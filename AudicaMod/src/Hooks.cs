﻿using HarmonyLib;
using UnityEngine;
using System;
using MelonLoader;
using System.Linq;
using static DifficultyCalculator;

namespace AudicaModding
{
    internal static class Hooks
    {
        private static int buttonCount = 0;

        /*
        public static void ApplyHooks(HarmonyInstance instance)
        {
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
        */

        [HarmonyPatch(typeof(OptionsMenu), "AddButton", new Type[] { typeof(int), typeof(string), typeof(OptionsMenuButton.SelectedActionDelegate), typeof(OptionsMenuButton.IsCheckedDelegate), typeof(string), typeof(OptionsMenuButton), })]
        private static class AddButtonButton
        {
            private static void Postfix(OptionsMenu __instance, int col, string label, OptionsMenuButton.SelectedActionDelegate onSelected, OptionsMenuButton.IsCheckedDelegate isChecked)
            {
                if (__instance.mPage == OptionsMenu.Page.Main)
                {
                    buttonCount++;
                    if (buttonCount == 9)
                    {
                        SongDownloaderUI.AddPageButton(__instance, 0);
                        SongSearchScreen.SetMenu(__instance);
                        PlaylistCreatePanel.SetMenu(__instance);
                        PlaylistSelectPanel.SetMenu(__instance);
                        PlaylistEditPanel.SetMenu(__instance);
                        PlaylistEndlessPanel.SetMenu(__instance);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "ShowPage", new Type[] { typeof(OptionsMenu.Page) })]
        private static class PatchShowOptionsPage
        {
            private static void Prefix(OptionsMenu __instance, OptionsMenu.Page page)
            {
                SongBrowser.shouldShowKeyboard = false;
                buttonCount = 0;
                SongDownloader.searchString = "";
            }
            private static void Postfix(InGameUI __instance, OptionsMenu.Page page)
            {
                if (page == OptionsMenu.Page.Main)
                {                   
                    if (SongSearch.searchInProgress)
                    {
                        SongSearchScreen.GoToSearch();
                    }
                    else if (PlaylistManager.state == PlaylistManager.PlaylistState.Selecting || PlaylistManager.state == PlaylistManager.PlaylistState.Adding)
                    {
                        PlaylistSelectPanel.GoToPanel();
                    }                       
                    else if(PlaylistManager.state == PlaylistManager.PlaylistState.Endless)
                    {
                        PlaylistEndlessPanel.GoToPanel();
                    }
                }
                if(page == OptionsMenu.Page.Misc)
                {
                    if (PlaylistManager.state == PlaylistManager.PlaylistState.Creating)
                    {
                        PlaylistCreatePanel.GoToPanel();
                    }
                    else if(PlaylistManager.state == PlaylistManager.PlaylistState.Editing)
                    {
                        PlaylistEditPanel.GoToPanel();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "BackOut", new Type[0])]
        private static class Backout
        {
            private static bool Prefix(OptionsMenu __instance)
            {
                // should always be on the search page when this happens
                if (SongSearch.searchInProgress)
                {
                    SongSearch.CancelSearch();
                    return false;
                }
                else if (PlaylistManager.state == PlaylistManager.PlaylistState.Selecting || PlaylistManager.state == PlaylistManager.PlaylistState.Adding)
                {
                    PlaylistSelectPanel.CancelSelect();
                    return false;
                }
                else if(PlaylistManager.state == PlaylistManager.PlaylistState.Creating)
                {
                    PlaylistCreatePanel.CancelCreate();
                }
                else if(PlaylistManager.state == PlaylistManager.PlaylistState.Editing)
                {
                    PlaylistEditPanel.CancelEdit();
                }
                else if(PlaylistManager.state == PlaylistManager.PlaylistState.Endless)
                {
                    PlaylistEndlessPanel.CancelEndless();
                }
                else
                {
                    if (SongDownloaderUI.songItemPanel != null)
                        SongDownloaderUI.songItemPanel.SetPageActive(false);
                    if (SongDownloader.needRefresh)
                        SongBrowser.ReloadSongList(false);
                }
                return true;
            }
        }        

        [HarmonyPatch(typeof(KeyboardEntry), "Hide", new Type[0])]
        private static class KeyboardEntry_Hide
        {
            private static bool Prefix(KeyboardEntry __instance)
            {
                if (SongBrowser.shouldShowKeyboard)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(KeyboardEntry), "OnKey", new Type[] { typeof(KeyCode), typeof(string) })]
        private static class KeyboardEntry_OnKey
        {
            private static bool Prefix(KeyboardEntry __instance, KeyCode keyCode, string label)
            {
                if (SongBrowser.shouldShowKeyboard)
                {
                    if(PlaylistManager.state == PlaylistManager.PlaylistState.Creating)
                    {
                        switch (label)
                        {
                            case "done":
                                __instance.Hide();
                                break;
                            case "clear":
                                PlaylistCreatePanel.newName = "";
                                break;
                            default:
                                PlaylistCreatePanel.newName += label;
                                break;
                        }
                        if(PlaylistCreatePanel.playlistText != null)
                        {
                            PlaylistCreatePanel.playlistText.text = PlaylistCreatePanel.newName;
                        }
                    }
                    
                    if (SongSearch.searchInProgress)
                    {
                        switch (label)
                        {
                            case "done":
                                __instance.Hide();
                                SongBrowser.shouldShowKeyboard = false;
                                SongSearch.OnNewUserSearch();
                                break;
                            case "clear":
                                    SongSearch.query = "";
                                break;
                            default:
                                    SongSearch.query += label;
                                break;
                        }

                        if (SongSearchScreen.searchText != null)
                        {
                            SongSearchScreen.searchText.text = SongSearch.query;
                        }
                    }
                    else
                    {
                        switch (label)
                        {
                            case "done":
                                __instance.Hide();
                                SongBrowser.shouldShowKeyboard = false;
                                SongDownloader.StartNewSongSearch();
                                break;
                            case "clear":
                                SongDownloader.searchString = "";
                                break;
                            default:
                                SongDownloader.searchString += label;
                                break;
                        }

                        if (SongDownloaderUI.searchText != null)
                        {
                            SongDownloaderUI.searchText.text = SongDownloader.searchString;
                        }
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(KeyboardEntry), "OnUnderscore", new Type[0])]
        private static class KeyboardEntry_UnderScore
        {
            private static bool Prefix(KeyboardEntry __instance)
            {
                if (SongBrowser.shouldShowKeyboard)
                {
                    if(PlaylistManager.state == PlaylistManager.PlaylistState.Creating)
                    {
                        PlaylistCreatePanel.newName += " ";

                        if (PlaylistCreatePanel.playlistText != null)
                        {
                            PlaylistCreatePanel.playlistText.text = PlaylistCreatePanel.newName;
                        }
                    }

                    if (SongSearch.searchInProgress)
                    {
                        SongSearch.query += " ";

                        if (SongSearchScreen.searchText != null)
                        {
                            SongSearchScreen.searchText.text = SongSearch.query;
                        }
                    }
                    else
                    {
                        SongDownloader.searchString += " ";

                        if (SongDownloaderUI.searchText != null)
                        {
                            SongDownloaderUI.searchText.text = SongDownloader.searchString;
                        }
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(KeyboardEntry), "OnBackspace", new Type[0])]
        private static class KeyboardEntry_BackSpace
        {
            private static bool Prefix(KeyboardEntry __instance)
            {
                if (SongBrowser.shouldShowKeyboard)
                {
                    if(PlaylistManager.state == PlaylistManager.PlaylistState.Creating)
                    {
                        if (PlaylistCreatePanel.newName == "" || PlaylistCreatePanel.newName is null)
                        {
                            return false;
                        }

                        PlaylistCreatePanel.newName = PlaylistCreatePanel.newName.Substring(0, PlaylistCreatePanel.newName.Length - 1);

                        if (PlaylistCreatePanel.playlistText != null)
                        {
                            PlaylistCreatePanel.playlistText.text = PlaylistCreatePanel.newName;
                        }
                            
                    }

                    if (SongSearch.searchInProgress)
                    {
                        if (SongSearch.query == "" || SongSearch.query == null)
                            return false;
                        SongSearch.query = SongSearch.query.Substring(0, SongSearch.query.Length - 1);

                        if (SongSearchScreen.searchText != null)
                        {
                            SongSearchScreen.searchText.text = SongSearch.query;
                        }
                    }
                    else
                    {
                        if (SongDownloader.searchString == "" || SongDownloader.searchString == null)
                            return false;
                        SongDownloader.searchString = SongDownloader.searchString.Substring(0, SongDownloader.searchString.Length - 1);
                        
                        if (SongDownloaderUI.searchText != null)
                        {
                            SongDownloaderUI.searchText.text = SongDownloader.searchString;
                        }
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(SongSelect), "GetSongIDs", new Type[] { typeof(bool) })]
        private static class FilterScrollerItems
        {
            private static void Postfix(SongSelect __instance, ref bool extras, ref Il2CppSystem.Collections.Generic.List<string> __result)
            {
                FilterPanel.ApplyFilter(__instance, ref extras, ref __result);
                if (SongBrowser.deletedSongs.Count > 0)
                {
                    foreach (var deletedSong in SongBrowser.deletedSongs)
                    {
                        __result.Remove(deletedSong);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SongSelect), "AddToScroller", new Type[] { typeof(SongSelect.SongSelectItemEntry) })]
        private static class ModifySongSelectEntryName
        {
            private static void Postfix(SongSelect __instance, SongSelect.SongSelectItemEntry entry)
            {
                var song = SongList.I.GetSong(entry.songID);
                if (entry.item.mapperLabel != null)
                {
                    //package data to be used for display
                    SongBrowser.SongDisplayPackage songd = new SongBrowser.SongDisplayPackage();

                    songd.hasEasy = song.hasEasy;
                    songd.hasStandard = song.hasNormal;
                    songd.hasAdvanced = song.hasHard;
                    songd.hasExpert = song.hasExpert;

                    //if song data loader is installed look for custom tags
                    if (SongBrowser.songDataLoaderInstalled)
                    {
                        songd = SongBrowser.SongDisplayPackage.FillCustomData(songd, song.songID);
                    }


                    CachedCalculation easy = DifficultyCalculator.GetRating(song.songID, KataConfig.Difficulty.Easy.ToString());
                    CachedCalculation normal = DifficultyCalculator.GetRating(song.songID, KataConfig.Difficulty.Normal.ToString());
                    CachedCalculation hard = DifficultyCalculator.GetRating(song.songID, KataConfig.Difficulty.Hard.ToString());
                    CachedCalculation expert = DifficultyCalculator.GetRating(song.songID, KataConfig.Difficulty.Expert.ToString());

                    //add mine tag if there are mines
                    if (song.hasEasy && easy.hasMines) songd.customEasyTags.Insert(0, "Mines");
                    if (song.hasNormal && normal.hasMines) songd.customStandardTags.Insert(0, "Mines");
                    if (song.hasHard && hard.hasMines) songd.customAdvancedTags.Insert(0, "Mines");
                    if (song.hasExpert && expert.hasMines) songd.customExpertTags.Insert(0, "Mines");

                    //add 360 tag
                    if (song.hasEasy && easy.is360) songd.customEasyTags.Insert(0, "360");
                    if (song.hasNormal && normal.is360) songd.customStandardTags.Insert(0, "360");
                    if (song.hasHard && hard.is360) songd.customAdvancedTags.Insert(0, "360");
                    if (song.hasExpert && expert.is360) songd.customExpertTags.Insert(0, "360");

                    songd.customExpertTags = songd.customExpertTags.Distinct().ToList();
                    songd.customStandardTags = songd.customStandardTags.Distinct().ToList();
                    songd.customAdvancedTags = songd.customAdvancedTags.Distinct().ToList();
                    songd.customEasyTags = songd.customEasyTags.Distinct().ToList();

                    entry.item.mapperLabel.text += SongBrowser.GetDifficultyString(songd);
                }
            }
        }

        [HarmonyPatch(typeof(MainMenuPanel), "OnEnable", new Type[0])]
        private static class PatchMainMenuPanel
        {
            private static void Postfix(MenuState __instance)
            {
                SongLoadingManager.UpdateUI();
            }
        }

        [HarmonyPatch(typeof(SongSelect), "ChangeSort", new Type[] { typeof(SongSelect.Sort)})]
        private static class PathSongSelectChangeSort
        {
            private static void Prefix(SongSelect __instance, ref SongSelect.Sort newSort)
            {
                if (FilterPanel.IsFiltering("playlists"))
                {
                    newSort = SongSelect.Sort.Default;
                }
            }
        }

        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
        private static class Patch2SetMenuState
        {
            private static void Postfix(MenuState __instance, ref MenuState.State state)
            {
                if (state == MenuState.State.LaunchPage)
                {
                    DeleteButton.CreateDeleteButton();
                    FavoriteButton.CreateFavoriteButton();
                    AddPlaylistButton.CreatePlaylistButton();
                    DifficultyDisplay.Show();
                }
                else
                {
                    DifficultyDisplay.Hide();
                }
                if (state == MenuState.State.SongPage)
                {
                    ScoreDisplayList.Show();
                    RandomSongButton.CreateRandomSongButton();
                    SongSearchButton.CreateSearchButton();
                    SelectPlaylistButton.CreatePlaylistButton();
                    PlaylistEndlessButton.CreatePlaylistButton();
                    RefreshButton.CreateRefreshButton();
                    PlaylistEndlessManager.ResetIndex();
                }
                else
                {
                    ScoreDisplayList.Hide();
                    PlaylistEndlessButton.HidePlaylistButton();
                }

                if(state == MenuState.State.MainPage)
                {
                    //PlaylistManager.DownloadMissingSongs();
                }
            }
        }

        [HarmonyPatch(typeof(SongEndSequence), "SetState", new Type[] {typeof(SongEndSequence.State)})]
        private static class PatchSongEndSequenceSetState
        {
            private static bool Prefix(SongEndSequence __instance, ref SongEndSequence.State newState)
            {
                if(PlaylistManager.state == PlaylistManager.PlaylistState.Endless)
                {
                    if (Config.ShowScores)
                    {
                        if(newState == SongEndSequence.State.WaitForScorePercentStars)
                        {
                            PlaylistEndlessManager.FadeOut();
                            return true;
                        }
                    }                    
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(SongEndSequence), "Start", new Type[0])]
        private static class PatchSongEndSequenceStart
        {
            private static bool Prefix(SongEndSequence __instance)
            {
                if(PlaylistManager.state == PlaylistManager.PlaylistState.Endless)
                {
                    if (!Config.ShowScores)
                    {
                        PlaylistEndlessManager.FadeOut();
                        __instance.startDelay = 0f;
                        __instance.waitDelay = 0f;
                        __instance.endDelay = 0f;
                        __instance.levelComplete.SetActive(false);
                        __instance.newHighScore.SetActive(false);
                        __instance.scorePercentStars.SetActive(false);
                        __instance.fullCombo.SetActive(false);
                        __instance.SetState(SongEndSequence.State.SequenceComplete);
                        return false;
                    }
                }             
                return true;
            }
        }

        /*[HarmonyPatch(typeof(HighScoreRecords), "RecordDifficultyScore", new Type[] {typeof(string), typeof(int), typeof(KataConfig.Difficulty), typeof(float), typeof(bool), typeof(bool)})]
        private static class PatchRecordDifficultyScore
        {
            private static void Postfix(HighScoreRecords __instance, string songID, int score, KataConfig.Difficulty difficulty, float percent, bool fullCombo, bool force)
            {
                MelonLogger.Msg("Score recorded");
            }
        }*/


        [HarmonyPatch(typeof(StartupLogo), "SetState", new Type[] { typeof(StartupLogo.State) })]
        private static class Patch2SetLogoState
        {
            private static void Postfix(StartupLogo __instance, ref StartupLogo.State state)
            {
                if (state == StartupLogo.State.Done)
                {
                    SongDownloader.StartNewSongSearch();
                    PlaylistManager.OnApplicationStart();
                    FilterPanel.OnApplicationStart();
                    SongLoadingManager.StartSongListUpdate();
                    //PlaylistManager.GetAllApiSongs();
                }
            }
        }

        [HarmonyPatch(typeof(InGameUI), "SetState", new Type[] { typeof(InGameUI.State), typeof(bool) })]
        private static class PatchSetInGameUIState
        {
            private static bool Prefix(InGameUI __instance, InGameUI.State state, bool instant)
            {
                if (PlaylistManager.state == PlaylistManager.PlaylistState.Endless)
                {
                    if (state == InGameUI.State.ResultsPage)
                    {
                        PlaylistEndlessManager.NextSong();
                        return false; //false = don't run function
                    }
                }

                return true;
            }

            private static void Postfix(InGameUI __instance, InGameUI.State state, bool instant)
            {
                if (state == InGameUI.State.FailedPage)
                {
                    DeleteButton.CreateDeleteButton(ButtonUtils.ButtonLocation.Failed);
                    FavoriteButton.CreateFavoriteButton(ButtonUtils.ButtonLocation.Failed);
                    AddPlaylistButton.CreatePlaylistButton(ButtonUtils.ButtonLocation.Failed);
                    PlaylistEndlessSkipButton.CreateSkipButton(ButtonUtils.ButtonLocation.Failed);
                }
                else if (state == InGameUI.State.PausePage)
                {
                    DeleteButton.CreateDeleteButton(ButtonUtils.ButtonLocation.Pause);
                    FavoriteButton.CreateFavoriteButton(ButtonUtils.ButtonLocation.Pause);
                    AddPlaylistButton.CreatePlaylistButton(ButtonUtils.ButtonLocation.Pause);
                    PlaylistEndlessSkipButton.CreateSkipButton(ButtonUtils.ButtonLocation.Pause);
                }
                else if (state == InGameUI.State.EndGameContinuePage)
                {
                    DeleteButton.CreateDeleteButton(ButtonUtils.ButtonLocation.EndGame);
                    FavoriteButton.CreateFavoriteButton(ButtonUtils.ButtonLocation.EndGame);
                    AddPlaylistButton.CreatePlaylistButton(ButtonUtils.ButtonLocation.EndGame);
                }    
                else if (state == InGameUI.State.PracticeModeOverPage)
                {
                    DeleteButton.CreateDeleteButton(ButtonUtils.ButtonLocation.PracticeModeOver);
                    FavoriteButton.CreateFavoriteButton(ButtonUtils.ButtonLocation.PracticeModeOver);
                    AddPlaylistButton.CreatePlaylistButton(ButtonUtils.ButtonLocation.PracticeModeOver);
                }
            }
        }

        [HarmonyPatch(typeof(SongSelect), "OnEnable", new Type[0])]
        private static class AdjustSongSelect
        {
            private static void Postfix(SongSelect __instance)
            {
                FilterPanel.Initialize();
                ScoreHistory.LoadHistory(PlatformChooser.I.GetLeaderboardID());
                MelonCoroutines.Start(SongBrowser.UpdateLastSongCount());
                MelonLogger.Msg("Updating song count");
            }
        }


        [HarmonyPatch(typeof(SongListControls), "FilterAll", new Type[0])]
        private static class FilterAll
        {
            private static void Prefix(SongListControls __instance)
            {
                FilterPanel.DisableCustomFilters();
            }
        }

        [HarmonyPatch(typeof(SongListControls), "FilterMain", new Type[0])]
        private static class FilterMain
        {
            private static void Prefix(SongListControls __instance)
            {
                FilterPanel.DisableCustomFilters();
            }
        }

        /// <summary>
        /// OnLeaderboardDataResponse() is also processed when the user is already in
        /// a song, which can lead to a lag spike. This is supposed to avoid that spike
        /// with minimal impact.
        /// </summary>
        [HarmonyPatch(typeof(SongSelect), "OnLeaderboardDataResponse", new Type[] { typeof(string) })]
        private static class StopLeaderboardUpdateForFriends
        {
            private static bool Prefix(SongSelect __instance, string response)

            {
                if (MenuState.GetState() == MenuState.State.Launched)
                {
                    return false;
                }
                return true;
            }
        }


    }
}
