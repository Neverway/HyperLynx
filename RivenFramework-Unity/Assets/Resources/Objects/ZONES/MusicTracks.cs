//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

public class MusicList : MonoBehaviour
{
    public List<MusicTrack> Musics;
    public GI_MusicManager musicManager;
    
    
    private void Start()
    {
        musicManager = GameInstance.Get<GI_MusicManager>();
    }

    public void CrossFadeToTrack(MusicTracks _musicTrack, float _cossfadeDuration, bool _playTrackFromStart=false)
    {
        switch (_musicTrack)
        {
            case MusicTracks.None:
                break;
            case MusicTracks.StopMusic:
                musicManager.CrossFadeToTrack(null, _cossfadeDuration, _playTrackFromStart);
                break;
            default:
                AudioClip track = null;
                foreach (var musicTrack in Musics)
                {
                    if (musicTrack.identifier == _musicTrack)
                    {
                        track = musicTrack.audioFile;
                        break;
                    }
                }
                musicManager.CrossFadeToTrack(track, _cossfadeDuration, _playTrackFromStart);
                break;
        }
    }
}

[Serializable]
public class MusicTrack
{
    public MusicTracks identifier;
    public AudioClip audioFile;
}

[Serializable]
public enum MusicTracks
{
    None,
    StopMusic,
    GameOver,
    Title,
    Desktop,
    DesktopCorrupt,
    TrashZone,
    TrashZoneBoss,
    FirewallZone,
    FirewallZoneBoss,
    Victory,
    VictoryEscape,
}