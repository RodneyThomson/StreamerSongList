# StreamerSongList
C# scripts and library for interfacing with StreamerSongList (https://www.streamersonglist.com/)

- /StreamerSongListLib : Core API interface. Includes methods for:
    - GetStreamerIdFromName(string streamerName) [static]
    - GetAllSongs(bool showInactive = true)
    - SearchSong(string searchText, bool showInactive = true)
    - GetAllAttributes()
    - AddAttribute(Song song, int attributeId)
    - SetPrice(Song song, float price)
- /AddAttributeToSongList : Adds the desired attribute to all songs titles provided in text file [WARNING - currently not working due to API bug - any previous attributes will be lost]
- /SetCostOnSongList : Sets the price for all songs titles provided in text file [WARNING - currently not working due to API bug - any attributes set on song will be lost]
- 

## Notes

- All API calls require Authorization header with a User Access Token (See how to get yours at https://dev.streamersonglist.com/docs/authentication)
