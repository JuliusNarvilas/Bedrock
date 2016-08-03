using System;
using System.Diagnostics;

namespace Common.Events
{
    /// <summary>
    /// Structure for wrapping simple event name string data with some other functionality.
    /// </summary>
    public struct EventName : IEquatable<EventName>
    {
        /// <summary>
        /// Event name string;
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Event name hash;
        /// </summary>
        public readonly int Hash;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventName"/> struct.
        /// </summary>
        /// <param name="i_Name">Name of the event.</param>
        public EventName(string i_Name)
        {
            Name = i_Name;
            Hash = i_Name.GetHashCode();
        }

        /// <summary>
        /// Equality comparison with another <see cref="EventName"/> struct.
        /// </summary>
        /// <param name="i_Other">The other event name.</param>
        /// <returns></returns>
        public bool Equals(EventName i_Other)
        {
#if DEBUG
            if (Hash == i_Other.Hash)
            {
                Debug.Assert(Name == i_Other.Name, "EventName \"" + Name + "\" Hash " + Hash + " incorrectley matched with EventName \"" + i_Other.Name + "\".");
                return true;
            }
            return false;
#else
            return Hash == i_Other.Hash;
#endif
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="EventName"/>.
        /// </summary>
        /// <param name="i_Name">Name of the event.</param>
        /// <returns>
        /// A new instance of <see cref="EventName"/>.
        /// </returns>
        public static implicit operator EventName(string i_Name)
        {
            return new EventName(i_Name);
        }
        /// <summary>
        /// Performs an implicit conversion from <see cref="EventName"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="i_EventName">Representation of the event name.</param>
        /// <returns>
        /// The event name string.
        /// </returns>
        public static implicit operator string(EventName i_EventName)
        {
            return i_EventName.Name;
        }
        /// <summary>
        /// Performs an implicit conversion from <see cref="EventName"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="i_EventName">Representation of the event name.</param>
        /// <returns>
        /// Hash of the event name string.
        /// </returns>
        public static implicit operator int(EventName i_EventName)
        {
            return i_EventName.Hash;
        }
    }

    /// <summary>
    /// Management class for event subscription and firing.
    /// </summary>
    public class EventManager
    {
        /// <summary>
        /// Definition of the event response interface.
        /// </summary>
        /// <param name="i_EventName">Name of the event.</param>
        /// <param name="i_Context">The event context.</param>
        public delegate void EventHandler(EventName i_EventName, object i_Context);

        /// <summary>
        /// Struct wrapper of <see cref="EventHandler"/> type event subscrition to bundle subscriptions in an array.
        /// </summary>
        private struct EventChannel
        {
            public event EventHandler Subscription;

            /// <summary>
            /// Fires the specified event.
            /// </summary>
            /// <param name="i_EventName">Name of the event.</param>
            /// <param name="i_EventContext">The event context.</param>
            public void Fire(EventName i_EventName, object i_EventContext)
            {
                if(Subscription != null)
                {
                    Subscription.Invoke(i_EventName, i_EventContext);
                }
            }
        }

        /// <summary>
        /// The subscriptions for all channels.
        /// </summary>
        private readonly EventChannel[] m_ChannelSubscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventManager"/> class.
        /// </summary>
        /// <param name="i_MaxChannels">The maximum number of channels.</param>
        public EventManager(int i_MaxChannels)
        {
            Debug.Assert(i_MaxChannels > 0 && i_MaxChannels <= (sizeof(long) * 8), "Max number of event channels must be a positive number");
            m_ChannelSubscriptions = new EventChannel[i_MaxChannels];
        }

        /// <summary>
        /// Subscribes to an event channel.
        /// </summary>
        /// <param name="i_ChannelIndex">Index of the channel.</param>
        /// <param name="i_Response">The event response.</param>
        public void SubscribeToChannel(int i_ChannelIndex, EventHandler i_Response)
        {
            Debug.Assert(i_ChannelIndex >= 0 && i_ChannelIndex < m_ChannelSubscriptions.Length, "Channel index out of bounds.");
            m_ChannelSubscriptions[i_ChannelIndex].Subscription += i_Response;
        }

        /// <summary>
        /// Subscribes to multiple event channels.
        /// </summary>
        /// <param name="i_ChannelMask">A mask for channel indices where set bit positions indicate indices (1 at position 0 indicates channel 0 is matched).</param>
        /// <param name="i_Response">The event response.</param>
        public void SubscribeToChannels(long i_ChannelMask, EventHandler i_Response)
        {
            int size = m_ChannelSubscriptions.Length;
            for (int i = 0; i < size; ++i)
            {
                long flag = 1L << i;
                if ((i_ChannelMask & flag) == flag)
                {
                    m_ChannelSubscriptions[i].Subscription += i_Response;
                }
            }
        }

        /// <summary>
        /// Unsubscribes from event channel.
        /// </summary>
        /// <param name="i_ChannelIndex">Index of the channel.</param>
        /// <param name="i_Response">The event response.</param>
        public void UnsubscribeFromChannel(int i_ChannelIndex, EventHandler i_Response)
        {
            Debug.Assert(i_ChannelIndex >= 0 && i_ChannelIndex < m_ChannelSubscriptions.Length, "Channel index out of bounds.");
            m_ChannelSubscriptions[i_ChannelIndex].Subscription -= i_Response;
        }

        /// <summary>
        /// Unsubscribes from event channels.
        /// </summary>
        /// <param name="i_ChannelMask">A mask for channel indices where set bit positions indicate indices (1 at position 0 indicates channel 0 is matched).</param>
        /// <param name="i_Response">The event response.</param>
        public void UnsubscribeFromChannels(long i_ChannelMask, EventHandler i_Response)
        {
            int size = m_ChannelSubscriptions.Length;
            for (int i = 0; i < size; ++i)
            {
                long flag = 1L << i;
                if ((i_ChannelMask & flag) == flag)
                {
                    m_ChannelSubscriptions[i].Subscription -= i_Response;
                }
            }
        }

        /// <summary>
        /// Unsubscribes from all event channels.
        /// </summary>
        /// <param name="i_Response">The event response.</param>
        public void UnsubscribeFromAllChannels(EventHandler i_Response)
        {
            int size = m_ChannelSubscriptions.Length;
            for (int i = 0; i < size; ++i)
            {
                m_ChannelSubscriptions[i].Subscription -= i_Response;
            }
        }

        /// <summary>
        /// Fires an event.
        /// </summary>
        /// <param name="i_ChannelIndex">Index of the event channel.</param>
        /// <param name="i_Name">Name of the event.</param>
        /// <param name="i_EventContext">The event context.</param>
        public void FireEvent(int i_ChannelIndex, EventName i_Name, object i_EventContext = null)
        {
            Debug.Assert(i_ChannelIndex >= 0 && i_ChannelIndex < m_ChannelSubscriptions.Length, "Channel index out of bounds.");
            m_ChannelSubscriptions[i_ChannelIndex].Fire(i_Name, i_EventContext);
        }

        /// <summary>
        /// Fires a multichannel event.
        /// </summary>
        /// <param name="i_ChannelMask">A mask for channel indices where set bit positions indicate indices (1 at position 0 indicates channel 0 is matched)</param>
        /// <param name="i_Name">Name of the event.</param>
        /// <param name="i_EventContext">The event context.</param>
        public void FireMultichannelEvent(long i_ChannelMask, EventName i_Name, object i_EventContext = null)
        {
            int size = m_ChannelSubscriptions.Length;
            for (int i = 0; i < size; ++i)
            {
                long flag = 1L << i;
                if((i_ChannelMask & flag) == flag)
                {
                    m_ChannelSubscriptions[i].Fire(i_Name, i_EventContext);
                }
            }
        }
    }
}
