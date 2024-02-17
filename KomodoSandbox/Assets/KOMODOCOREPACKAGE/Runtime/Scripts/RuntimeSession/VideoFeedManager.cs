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
        GameObject videoFeed;

        // Attempt to find an inactive feed first
        if (inactiveFeeds.Count > 0)
        {
            videoFeed = inactiveFeeds[0];
            inactiveFeeds.RemoveAt(0);
        }
        else
        {
            // Create new feed if no inactive ones are available
            videoFeed = Instantiate(videoPrefab, videoGridParent);
        }

        videoFeed.SetActive(true);
        
        if(activeVideoFeeds.ContainsKey(clientID))
          activeVideoFeeds[clientID] = videoFeed;
        else
            activeVideoFeeds.Add(clientID, videoFeed);


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
