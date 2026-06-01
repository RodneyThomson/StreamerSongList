# StreamerSongList
C# scripts and library for interfacing with StreamerSongList (https://www.streamersonglist.com/)

- /StreamerSongListLib : Core API interface. Includes methods for:
    - GetStreamerIdFromName(string streamerName) [static]
    - SearchSong(string searchText, bool showInactive = true)
    - GetAllAttributes()
    - AddAttribute(Song song, int attributeId)
- /AddAttributeToSongList : Adds the desired attribute to all songs titles provided in text file

## Notes

- Assumes all API calls require Authorization header and accompanying access token (they don't - but anything that requires write permissions, or mod+ privileges such as searching inactive files DOES)
- Access token can be read by navigating to https://www.streamersonglist.com/, logging in with your privileged account, and then looking at the Fetch/XHR requests in Developer Tools (See the Authorization request header)
- Alternatively in the browser Developer Tools console enter: 
`console.log(localStorage.getItem('StreamerSonglist_authToken'))`
