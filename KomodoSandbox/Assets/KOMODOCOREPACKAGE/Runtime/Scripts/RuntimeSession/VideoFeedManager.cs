using UnityEngine;
using System.Collections.Generic;

public class VideoFeedManager : MonoBehaviour
{
    [SerializeField] private GameObject videoPrefab;
    [SerializeField] private Transform videoGridParent;
    public Dictionary<int, GameObject> activeVideoFeeds = new Dictionary<int, GameObject>();
    public List<GameObject> inactiveFeeds = new List<GameObject>();

    public (int clientID, GameObject videoFeed) GetOrCreateVideoFeed(int clientID)
    {
        // Check if the video feed for the clientID already exists and return it if so
        if (activeVideoFeeds.TryGetValue(clientID, out GameObject existingFeed))
        {
            return (clientID, existingFeed); // Return the existing feed without creating a new one
        }

        // If no existing feed, attempt to find an inactive feed
        GameObject videoFeed;
        if (inactiveFeeds.Count > 0)
        {
            videoFeed = inactiveFeeds[0];
            inactiveFeeds.RemoveAt(0);
        }
        else
        {
            // Create a new feed if no inactive ones are available
            videoFeed = Instantiate(videoPrefab, videoGridParent);
        }

        videoFeed.SetActive(true);
        activeVideoFeeds[clientID] = videoFeed; // Add or update the feed associated with the clientID

        // Return both the clientID and the videoFeed as a tuple
        return (clientID, videoFeed);
    }

    public void RemoveVideoFeed(int clientID)
    {
        if (activeVideoFeeds.TryGetValue(clientID, out GameObject videoFeed))
        {
            videoFeed.SetActive(false);
            activeVideoFeeds.Remove(clientID);
            inactiveFeeds.Add(videoFeed);
        }
    }

    public void DeactivateAllVideoFeeds()
    {
        foreach (var feed in activeVideoFeeds.Values)
        {
            feed.SetActive(false);
            inactiveFeeds.Add(feed);
        }
        activeVideoFeeds.Clear();
    }
}
