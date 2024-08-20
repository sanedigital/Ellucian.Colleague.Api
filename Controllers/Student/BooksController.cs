// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Book information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class BooksController : BaseCompressedApiController
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        /// <summary>
        /// BooksController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="bookRepository"></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BooksController(IAdapterRegistry adapterRegistry, IBookRepository bookRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _bookRepository = bookRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves information about a specific book.
        /// </summary>
        /// <param name="id">Unique system Id of the book</param>
        /// <returns>Information about a specific <see cref="Book">Book</see></returns>
        /// <note>Book information is cached for 24 hours.</note>
        // [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        [HttpGet]
        [HeaderVersionRoute("/books/{id}", 1, true, Name = "GetBook")]
        public async Task<ActionResult<Book>> GetAsync(string id)
        {
            try
            {
                var book = await _bookRepository.GetAsync(id);
                if (book == null)
                {
                    return CreateNotFoundException("book", id);
                }
                // Get the right adapter for the type mapping
                var bookDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Book, Book>();

                // Map the book entity to the book DTO
                return bookDtoAdapter.MapToType(book);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
                _logger.LogError(csse, invalidSessionErrorMessage);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (System.Exception e)
            {
                this._logger.LogError(e, "Unable to retrieve the book information");
                return CreateHttpResponseException("Unable to retrieve data");
            }
        }

        /// <summary>
        /// Accepts a list of book IDs and/or a query string to be used against book titles, authors, and International Standard Book Numbers (ISBN) to post a query against books.
        /// </summary>
        /// <param name="criteria"><see cref="BookQueryCriteria">Query Criteria</see> including the list of Book Ids to use to retrieve books.</param>
        /// <returns>List of <see cref="Book">Book</see> objects. </returns>
        /// <note>Book information is cached for 24 hours.</note>
        [HttpPost]
        [HeaderVersionRoute("/qapi/books", 1, true, Name = "QueryBooks")]
        public async Task<ActionResult<IEnumerable<Book>>> QueryBooksByPostAsync([FromBody] BookQueryCriteria criteria)
        {
            try
            {
                var bookDtos = new List<Book>();
                if (criteria == null || (criteria.Ids == null && string.IsNullOrWhiteSpace(criteria.QueryString)))
                {
                    string errmsg = "Must provide either a list of Ids or a query string to query books";
                    _logger.LogError(errmsg);
                    return CreateHttpResponseException(errmsg, HttpStatusCode.BadRequest);
                }
                bool useCache = true;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        useCache = false;
                    }
                }
                List<Domain.Student.Entities.Book> bookEntities = new List<Domain.Student.Entities.Book>();

                // If specific IDs are given, use those
                if (criteria.Ids != null && criteria.Ids.Any())
                {
                    if (useCache)
                    {
                        // This will first get items from cache and any ones not found in cache will be looked for directly in db...
                        bookEntities.AddRange(await _bookRepository.GetBooksByIdsAsync(criteria.Ids));
                    }
                    else
                    {
                        // Gets all items directly from db for most up to date data.
                        bookEntities.AddRange(await _bookRepository.GetNonCachedBooksByIdsAsync(criteria.Ids));
                    }
                }
                // If a query string is given, use it as well
                if (!string.IsNullOrWhiteSpace(criteria.QueryString))
                {
                    bookEntities.AddRange(await _bookRepository.GetBooksByQueryStringAsync(criteria.QueryString));
                }

                // Remove duplicate entries
                bookEntities = bookEntities.Distinct().ToList();

                // Build DTOs from matching Book entities
                var bookDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Book, Book>();
                foreach (var book in bookEntities)
                {
                    bookDtos.Add(bookDtoAdapter.MapToType(book));
                }
                return bookDtos;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
                _logger.LogError(csse, invalidSessionErrorMessage);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ex)
            {
                string errorMessage = "Arguments were not properly provided to retrieve books details";
                _logger.LogError(ex, errorMessage);
                return CreateHttpResponseException(errorMessage, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                string errorMessage = "Exception occurred while retrieving books details";
                _logger.LogError(ex, errorMessage);
                return CreateHttpResponseException(errorMessage, HttpStatusCode.BadRequest);
            }
        }
    }
}
