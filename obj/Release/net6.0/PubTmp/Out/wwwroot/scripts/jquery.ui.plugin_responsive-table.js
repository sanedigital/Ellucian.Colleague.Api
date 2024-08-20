(function ($) {
    /// Makes a table responsive.  This function determines if the supplied elements are <table> elements, 
    /// then iterates over each one, grabbing the values of the <th> elements in the <thead> section and 
    /// using them as labels for the corresponding <td> elements in the <tbody> section of the table.  
    ///
    /// For the <tfoot> section, add "data-role" properties to each <td> element to give each one a 
    /// column heading on mobile devices. 
    ///
    /// A CSS class is assigned to each table after DOM inspection to provide the appropriate styling 
    /// and breakpoints for the table.  A "suppress-on-mobile" class can be added to any <td> element
    /// to hide that element on mobile devices.
    ///
    /// For proper rendering on smaller screens, cells with no text will be appended with an <span> 
    /// containing "&nbsp". This plug-in and its associated CSS use CSS3 generated content that will 
    //  not display nicely when <td> elements are empty.
    ///
    /// Valid Table Mark-Up:
    ///
    ///  <table id="tableId">
    ///     <caption>Table Caption</caption>
    ///     <thead>
    ///          <tr>
    ///              <th>Column Heading 1</th>
    ///              <th>Column Heading 2</th>    
    ///              <th>Column Heading 3</th>
    ///          </tr>
    ///      </thead>
    ///      <tbody>
    ///          <tr>
    ///              <td>Row 1 Cell 1</td>
    ///              <td>Row 1 Cell 2</td>
    ///              <td>Row 1 Cell 3</td>
    ///          </tr>
    ///      </tbody>
    ///      <tfoot>
    ///          <tr>
    ///              <td data-role="Footer Heading 1">Row 1 Cell 1</td>
    ///              <td data-role="Footer Heading 2">Row 1 Cell 2</td>
    ///              <td data-role="Footer Heading 3">Row 1 Cell 3</td>
    ///          </tr>
    ///      </tfoot>
    ///  </table>

    $.fn.makeTableResponsive = function () {

        var responsiveTableClass = "responsiveTable";

        // Confirm that the jQuery object is a table
        if (this.is("table")) {

            // Determine if at least one table element has been supplied
            if (this.length > 0) {

                // Iterate over each table in the supplied collection
                for (t = 0; t < this.length; t++) {

                    // Determine the number of columns and non-header rows in this table
                    var numberOfColumns = $(this[t]).find('tr')[0].cells.length;
                    var numberOfRows = $(this[t]).find('tbody tr').length;

            // Initialize the header label array
            var headerLabels = [];

                    // Ensure that this table has at least one non-header row and one column
            if (numberOfColumns > 0 && numberOfRows > 0) {

                // Add table header labels (i.e. <th> element values) to the array
                for (columnNumber = 1; columnNumber <= numberOfColumns; columnNumber++) {
                    var th = $(this[t]).find('thead tr th:nth-child(' + columnNumber + ')');

                    // If a table header has the "blank" class then use a blank space 
                    // Note: this is done for accessibility purposes; <th> elements should never be blank
                    if ($(th).find('.offScreen').length != 0) {
                        headerLabels.push('');
                    } else {
                        headerLabels.push($(th).html());
                    }
                }
           
                // Assign header labels as data-role attributes on appropriate <td> elements
                for (rowNumber = 1; rowNumber <= numberOfRows; rowNumber++) {
                    for (columnNumber = 0; columnNumber < numberOfColumns; columnNumber++) {
                        // <td> elements are 1-indexed but our column counter is 0-indexed, so our <td> counter must be incremented by 1
                        var tdIndex = columnNumber + 1;
                                var tdElement = $(this[t]).find('tbody tr:nth-child(' + rowNumber + ') td:nth-child(' + tdIndex + ')');
                                if (!tdElement.attr('data-role')) {
                                    tdElement.attr('data-role', headerLabels[columnNumber]);

                                    // Force a refresh of the <td> element so that column header shows in all browsers
                                    tdElement.addClass("force-refresh");
                                    tdElement.removeClass("force-refresh");
                                }

                                // If a <td> element is empty, add a space to give it content so that formatting is preserved when the table is flipped
                                if ($.trim(tdElement.html()) == '')  {
                                    tdElement.append("<span>&nbsp</span>");
                                }

                                // wrap the contents of the td for height fix if it hasn't already been wrapped
                                if ((tdElement).find('div.layout-table-cell').length == 0) {
                                    tdElement.wrapInner("<div class='layout-table-cell'></div>");
                                }
                            }
                        }
                    }
                }
            }

            // Add the responsive table CSS class to the table for styling and mobile breakpoints
            $(this).addClass(responsiveTableClass);
        }
    }
}(jQuery));

