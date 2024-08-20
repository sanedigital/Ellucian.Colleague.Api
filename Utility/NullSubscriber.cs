using NUglify.JavaScript.Syntax;
using StackExchange.Redis;
using System.Net;

namespace Ellucian.Colleague.Api.Utility
{
    /// <summary>
    /// Container for DI because we may not always have a subscriber
    /// </summary>
    public class NullSubscriber : ISubscriber
    {
        /// <summary>
        /// Always null, don't use
        /// </summary>
        public IConnectionMultiplexer Multiplexer => null;

        /// <summary>
        /// Don't use
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public EndPoint IdentifyEndpoint(RedisChannel channel, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Don't use
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<EndPoint> IdentifyEndpointAsync(RedisChannel channel, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsConnected(RedisChannel channel = default)
        {
            return false;
        }
        /// <summary>
        /// Don't Use
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public TimeSpan Ping(CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<TimeSpan> PingAsync(CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public long Publish(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<long> PublishAsync(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="handler"></param>
        /// <param name="flags"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ChannelMessageQueue Subscribe(RedisChannel channel, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="handler"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<ChannelMessageQueue> SubscribeAsync(RedisChannel channel, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public EndPoint SubscribedEndpoint(RedisChannel channel)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool TryWait(Task task)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="handler"></param>
        /// <param name="flags"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Unsubscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler = null, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flags"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void UnsubscribeAll(CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task UnsubscribeAllAsync(CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="handler"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task UnsubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler = null, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Wait(Task task)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public T Wait<T>(Task<T> task)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tasks"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WaitAll(params Task[] tasks)
        {
            throw new NotImplementedException();
        }
    }
}
