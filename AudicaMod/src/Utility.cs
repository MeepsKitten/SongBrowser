﻿using MelonLoader.TinyJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AudicaModding
{
    internal static class Utility
    {
        public static void EmptyDownloadsFolder()
        {
            String directoryName = Application.dataPath + @"\StreamingAssets\HmxAudioAssets\songs";
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            // first remove all files that were marked for deletion by the user
            if (File.Exists(SongBrowser.deletedDownloadsListPath))
            {
                string       text    = File.ReadAllText(SongBrowser.deletedDownloadsListPath);
                List<string> deleted = JSON.Load(text).Make<List<string>>();

                foreach (string fileName in deleted)
                {
                    string path = Path.Combine(SongBrowser.downloadsDirectory, fileName);
                    if (File.Exists(path))
                        File.Delete(path);
                }
                File.Delete(SongBrowser.deletedDownloadsListPath);
            }

            var dirInfo = new DirectoryInfo(directoryName);
            List<String> AudicaFiles = Directory
                               .GetFiles(SongBrowser.downloadsDirectory, "*.*", SearchOption.TopDirectoryOnly).ToList();
            foreach (string file in AudicaFiles)
            {
                FileInfo audicaFile = new FileInfo(file);
                if (new FileInfo(dirInfo + "\\" + audicaFile.Name).Exists == false)
                {
                    audicaFile.MoveTo(dirInfo + "\\" + audicaFile.Name);
                }
                else
                {
                    File.Delete(file);
                }
            }
            SongBrowser.emptiedDownloadsFolder = true;
        }
    }
}
