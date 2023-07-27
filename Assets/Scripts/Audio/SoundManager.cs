using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SoundManager : NetworkBehaviour
{
	[SerializeField] public AudioClip cardHover;
	[SerializeField] public AudioClip cardHighlight;
	[SerializeField] public AudioClip cardUnhighlight;
	[SerializeField] AudioClip cardFlip;
	[SerializeField] AudioClip menuClose;
	[SerializeField] AudioClip menuOpen;
	[SerializeField] AudioClip moveError;
	[SerializeField] AudioClip moveSuccess;
   	[SerializeField] List<AudioSource> audioSourcesPool = new List<AudioSource>();
	[SerializeField] AudioSource audioSourceEffect;
    int audioSourcePoolIndex = 0;
    void Awake()
    {
        Singleton = this;
    }
    void Start()
    {
        Deck.cardDeal += OnCardDeal;
		GameLogic.moveError += OnMoveError;
		GameLogic.moveSuccess += OnMoveSuccess;
		GameMenuUI.MenuOpen += OnMenuOpen;
		GameMenuUI.MenuClose += OnMenuClose;
    }
	public void PlayAudio(AudioClip clip)
	{
		audioSourceEffect.clip = clip;
		audioSourceEffect.Play();
	}
	void OnMenuClose(object sender, System.EventArgs e)
	{
		audioSourceEffect.clip = menuClose;
		audioSourceEffect.Play();
	}
	void OnMenuOpen(object sender, System.EventArgs e)
	{
		audioSourceEffect.clip = menuOpen;
		audioSourceEffect.Play();
	}
	void OnMoveError(object sender, System.EventArgs e)
	{
		audioSourceEffect.clip = moveError;
		audioSourceEffect.Play();
	}
	void OnMoveSuccess(object sender, System.EventArgs e) => OnMoveSuccessServerRpc(); 
	[ServerRpc(RequireOwnership = false)] void OnMoveSuccessServerRpc() => OnMoveSuccessClientRpc();
	[ClientRpc] void OnMoveSuccessClientRpc()
	{
		audioSourceEffect.clip = moveSuccess;
		audioSourceEffect.Play();
	}
    private void OnCardDeal(object sender, System.EventArgs e)
    {
        // Play the audio using one of the available audio sources from the pool
        AudioSource source = GetAvailableAudioSource();
        if (source != null)
            source.Play();
    }
	public override void OnDestroy()
	{
		Deck.cardDeal -= OnCardDeal;
		GameLogic.moveError -= OnMoveError;
		GameLogic.moveSuccess -= OnMoveSuccess;
	}
    private AudioSource GetAvailableAudioSource()
    {
        // Find an available audio source from the pool
        int startIndex = audioSourcePoolIndex;
        do
        {
            if (!audioSourcesPool[audioSourcePoolIndex].isPlaying)
                return audioSourcesPool[audioSourcePoolIndex];

            audioSourcePoolIndex = (audioSourcePoolIndex + 1) % audioSourcesPool.Count;
        } while (startIndex != audioSourcePoolIndex); // Make sure we don't end up in an infinite loop

        // No available audio source found, return null
        return null;
    }
    public static SoundManager Singleton;
}
