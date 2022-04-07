using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;

public class BookmarkManager : MonoBehaviour
{
    // ***** Singleton *****
    public static BookmarkManager Instance { get; private set; }

    private List<GameObject> bookmarks; // we keep a list of bookmark objects because if a video is deleted, need to delete all associated bookmarks
    public GameObject BookmarkPrefab;

    public GameObject BookmarkToolbar;  // UI for deleting
    private List<GameObject> selectedBookmarks;

    public void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        bookmarks = new List<GameObject>();
        selectedBookmarks = new List<GameObject>();
    }

    public void AddBookmark(VideoController videoController)
    {
        Transform activeDoodleParent = DoodleManager.Instance.GetActiveDoodleParent();
        GameObject newBookmark = Instantiate(BookmarkPrefab, activeDoodleParent.position, activeDoodleParent.rotation, activeDoodleParent.transform);
        bookmarks.Add(newBookmark);

        BookmarkController bookmarkController = newBookmark.GetComponent<BookmarkController>();
        bookmarkController.SetCheckpointTime(videoController.GetVideoTime());
        bookmarkController.videoController = videoController;
    }

    public void DeleteBookmark(GameObject bookmark)
    {
        if (bookmarks.Contains(bookmark))
        {
            bookmarks.Remove(bookmark);
            Destroy(bookmark);
        }
        else Debug.LogError("Bookmark not in list");
    }

    public void DeleteBookmarks()   // when delete button in UI is pressed
    {
        List<GameObject> selectedBookmarksCopy = new List<GameObject>();
        foreach (GameObject bookmark in selectedBookmarks) { selectedBookmarksCopy.Add(bookmark); }

        foreach (GameObject bookmark in selectedBookmarksCopy)
        {
            bookmarks.Remove(bookmark);
            Destroy(bookmark);
        }
        selectedBookmarks.Clear();
    }

    public void DeleteAllBookmarksForVideo(VideoController videoController)
    {
        Debug.Log("deleting all bookmarks for video!");
        List<GameObject> bookmarksToDelete = new List<GameObject>();  // make a copy so we can loop without error
        foreach (GameObject bookmark in bookmarks)
        {
            Debug.Log("looping");
            if (bookmark.GetComponent<BookmarkController>().videoController == videoController) // TODO figure out why this doesn't add anything to the list
            {
                bookmarksToDelete.Add(bookmark);
            }
        }
        Debug.Log(bookmarksToDelete.Count);
        foreach (GameObject bookmark in bookmarksToDelete) { DeleteBookmark(bookmark); }

        VideoManager.Instance.isDeletingBookmarks = false; // signal to VideoManager that we're done deleting, can delete the video now
    }


    public void SelectBookmark(GameObject bookmark)
    {
        if (!selectedBookmarks.Contains(bookmark)) { selectedBookmarks.Add(bookmark); }
    }

    public void DeselectBookmark(GameObject bookmark)
    {
        if (selectedBookmarks.Contains(bookmark)) { selectedBookmarks.Remove(bookmark); }
    }

    public void DeselectAllBookmarks()
    {
        List<GameObject> selectedBookmarksCopy = new List<GameObject>();
        foreach (GameObject bookmark in selectedBookmarks) { selectedBookmarksCopy.Add(bookmark); }

        foreach (GameObject bookmark in selectedBookmarksCopy) { bookmark.GetComponent<LeanSelectable>().Deselect(); }
        selectedBookmarks.Clear();
        HideBookmarkToolbar();
    }

    public void ShowBookmarkToolbar() { BookmarkToolbar.SetActive(true); }

    public void HideBookmarkToolbar()
    {
        if (selectedBookmarks.Count == 0)
        {
            BookmarkToolbar.SetActive(false);
        }
    }

    // FOR THE SAMPLE SCENE (doodles are hard coded in Unity, but need to add them to the bookmarks list so they can be deleted)
    public void AddPrecreatedDoodles(GameObject bookmark)
    {
        bookmarks.Add(bookmark);
    }
}
