using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BlackService.PubSubServer
{
    public class Filter
    {
        static Dictionary<string, List<EndPoint>> _subscriberList = new Dictionary<string, List<EndPoint>>();
        static public Dictionary<string, List<EndPoint>> SubscriberList
        {
            get
            {
                lock (typeof(Filter))
                {
                    return _subscriberList;
                }
            }
        }

        static public List<EndPoint> GetSubscribers(string topicName)
        {
            lock (typeof(Filter))
            {
                if (SubscriberList.ContainsKey(topicName))
                {
                    return SubscriberList[topicName];
                }
                else
                    return null;
            }
        }

        static public void AddSubscriber(string topicName, EndPoint subscriberEndpoint)
        {
            lock (typeof(Filter))
            {
                if (SubscriberList.ContainsKey(topicName))
                {
                    if (!SubscriberList[topicName].Contains(subscriberEndpoint))
                    {
                        SubscriberList[topicName].Add(subscriberEndpoint);
                    }
                }
                else
                {
                    List<EndPoint> newSubsscribeList = new List<EndPoint>();
                    newSubsscribeList.Add(subscriberEndpoint);
                    SubscriberList.Add(topicName, newSubsscribeList);
                }
            }
        }

        static public void RemoveSubscriber(string topicName, EndPoint subscriberEndpoint)
        {
            lock (typeof(Filter))
            {
                if (SubscriberList.ContainsKey(topicName))
                {
                    if (SubscriberList[topicName].Contains(subscriberEndpoint))
                    {
                        SubscriberList[topicName].Remove(subscriberEndpoint);
                    }
                }
            }
        }

    }
}
