#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Extensions/FileSignatureExtensions.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ATSPM.Domain.Extensions
{
    /// <summary>
    /// Filesignature structure
    /// <see href="https://en.wikipedia.org/wiki/List_of_file_signatures"/>
    /// </summary>
    public struct FileSignature
    {
        /// <summary>
        /// Filesignature
        /// </summary>
        /// <param name="magicHeader">File Magic Header</param>
        /// <param name="offset">Offset from start byte</param>
        /// <param name="extension">File extension</param>
        /// <param name="description">File type description</param>
        /// <param name="isCompressed">True if file type is compressed</param>
        public FileSignature(byte[] magicHeader, int offset, string extension, string description, bool isCompressed = false) =>
            (MagicHeader, Offset, Extension, Description, IsCompressed) = (magicHeader, offset, extension, description, isCompressed);

        /// <summary>
        /// File Magic Header
        /// </summary>
        public byte[] MagicHeader { get; set; }

        /// <summary>
        /// Offset from start byte
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// File extension
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// File type description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// True if file type is compressed
        /// </summary>
        public bool IsCompressed { get; set; }
    }

    /// <summary>
    /// FileSignature Extensions
    /// <see href="https://en.wikipedia.org/wiki/List_of_file_signatures"/>
    /// </summary>
    public static class FileSignatureExtensions
    {
        /// <summary>
        /// <see cref="FileSignature"/> List
        /// <see href="https://en.wikipedia.org/wiki/List_of_file_signatures"/>
        /// </summary>
        public static HashSet<FileSignature> FileSignatures => new()
        {
            { new FileSignature(new byte[] {0x4B, 0x57, 0x41, 0x4A }, 0, ".??_", "Windows 3.1x Compressed File", true)},
            { new FileSignature(new byte[] {0x53, 0x5A, 0x44, 0x44 }, 0, ".??_", "Windows 9x Compressed File", true)},
            { new FileSignature(new byte[] {0x53, 0x5A, 0x44, 0x44, 0x88, 0xF0, 0x27, 0x33 }, 0, ".??_", "Microsoft compressed file in Qua", true)},
            { new FileSignature(new byte[] {0x66, 0x74, 0x79, 0x70, 0x33, 0x67 }, 4, ".3gp", "3rd Generation Partnership Proje", false)},
            { new FileSignature(new byte[] {0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C }, 0, ".7z", "7-Zip File Format", false)},
            { new FileSignature(new byte[] {0x38, 0x53, 0x56, 0x58 }, 0, ".8sv", "", false)},
            { new FileSignature(new byte[] {0x2A, 0x2A, 0x41, 0x43, 0x45, 0x2A, 0x2A }, 0, ".ace", "ACE (compressed file format)", true)},
            { new FileSignature(new byte[] {0x41, 0x46, 0x46 }, 0, ".aff", "Advanced Forensics Format", false)},
            { new FileSignature(new byte[] {0x41, 0x49, 0x46, 0x46 }, 0, ".aif", "", false)},
            { new FileSignature(new byte[] {0x62, 0x6F, 0x6F, 0x6B, 0x00, 0x00, 0x00, 0x00 }, 0, ".alias", "macOS file Alias[54] (Symbolic l", false)},
            { new FileSignature(new byte[] {0x60, 0xEA }, 0, ".arj", "ARJ", false)},
            { new FileSignature(new byte[] {0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11 }, 0, ".asf", "Advanced Systems Format[22]", false)},
            { new FileSignature(new byte[] {0x4F, 0x62, 0x6A, 0x01 }, 0, ".avro", "Apache Avro binary file format", false)},
            { new FileSignature(new byte[] {0x42, 0x41, 0x43, 0x4B, 0x4D, 0x49, 0x4B, 0x45 }, 0, ".bac", "AmiBack Amiga Backup data file", false)},
            { new FileSignature(new byte[] {0x53, 0x50, 0x30, 0x31 }, 0, ".bin", "Amazon Kindle Update Package [6]", false)},
            { new FileSignature(new byte[] {0x42, 0x4C, 0x45, 0x4E, 0x44, 0x45, 0x52 }, 0, ".blend", "Blender File Format[67]", false)},
            { new FileSignature(new byte[] {0x42, 0x4D }, 0, ".bmp", "BMP file, a bitmap format used m", false)},
            { new FileSignature(new byte[] {0x42, 0x50, 0x47, 0xFB }, 0, ".bpg", "Better Portable Graphics format[", false)},
            { new FileSignature(new byte[] {0x42, 0x5A, 0x68 }, 0, ".bz2", "Compressed file using Bzip2 algo", true)},
            { new FileSignature(new byte[] { 0x49, 0x53, 0x63, 0x28 }, 0, ".cab", "InstallShield CAB Archive File", false)},
            { new FileSignature(new byte[] { 0x4D, 0x53, 0x43, 0x46 }, 0, ".cab", "Microsoft Cabinet file", false)},
            { new FileSignature(new byte[] { 0x49, 0x54, 0x53, 0x46, 0x03, 0x00, 0x00, 0x00 }, 0, ".chm", "MS Windows HtmlHelp Data", false)},
            { new FileSignature(new byte[] { 0x80, 0x2A, 0x5F, 0xD7 }, 0, ".cin", "Kodak Cineon image", false)},
            { new FileSignature(new byte[] { 0xCA, 0xFE, 0xBA, 0xBE }, 0, ".class", "Java class file, Mach - O Fat Bina", false)},
            { new FileSignature(new byte[] { 0xC9 }, 0, ".com", "CP / M 3 and higher with overlays[", false)},
            { new FileSignature(new byte[] { 0x49, 0x49, 0x2A, 0x00, 0x10, 0x00, 0x00, 0x00 }, 0, ".cr2", "Canon RAW Format Version 2[9]", false)},
            { new FileSignature(new byte[] { 0x43, 0x72, 0x32, 0x34 }, 0, ".crx", "Google Chrome extension[30] or p", false)},
            { new FileSignature(new byte[] { 0x05, 0x07, 0x00, 0x00, 0x42, 0x4F, 0x42, 0x4F }, 0, ".cwk", "AppleWorks 5 document", false)},
            { new FileSignature(new byte[] { 0x06, 0x07, 0xE1, 0x00, 0x42, 0x4F, 0x42, 0x4F }, 0, ".cwk", "AppleWorks 6 document", false)},
            { new FileSignature(new byte[] { 0x44, 0x41, 0x41 }, 0, ".daa", "Direct Access Archive PowerISO", false)},
            { new FileSignature(new byte[] { 0x50, 0x4D, 0x4F, 0x43, 0x43, 0x4D, 0x4F, 0x43 }, 0, ".dat", "Windows Files And Settings Trans", false)},
            { new FileSignature(new byte[] { 0x72, 0x65, 0x67, 0x66 }, 0, ".dat", "Windows Registry file", false)},
            { new FileSignature(new byte[] { 0x00, 0x01, 0x42, 0x44 }, 0, ".DBA", "Palm Desktop To Do Archive", false)},
            { new FileSignature(new byte[] { 0xBE, 0xBA, 0xFE, 0xCA }, 0, ".DBA", "Palm Desktop Calendar Archive", false)},
            { new FileSignature(new byte[] { 0x44, 0x49, 0x43, 0x4D }, 128, ".dcm", "DICOM Medical File Format", false)},
            { new FileSignature(new byte[] { 0x21, 0x3C, 0x61, 0x72, 0x63, 0x68, 0x3E, 0x0A }, 0, ".deb", "linux deb file", false)},
            { new FileSignature(new byte[] { 0x30, 0x82 }, 0, ".der", "DER encoded X.509 certificate", false)},
            { new FileSignature(new byte[] { 0x64, 0x65, 0x78, 0x0A, 0x30, 0x33, 0x35, 0x00 }, 0, ".dex", "Dalvik Executable", false)},
            { new FileSignature(new byte[] { 0x41, 0x54, 0x26, 0x54, 0x46, 0x4F, 0x52, 0x4D }, 0, ".djvu", "DjVu document", false)},
            { new FileSignature(new byte[] { 0x6B, 0x6F, 0x6C, 0x79 }, 0, ".dmg", "Apple Disk Image file", false)},
            { new FileSignature(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, 0, ".doc", "Compound File Binary Format, a c", false)},
            { new FileSignature(new byte[] { 0x44, 0x52, 0x41, 0x43, 0x4F }, 0, ".drc", "3D model compressed with Google", true)},
            { new FileSignature(new byte[] { 0x45, 0x56, 0x46 }, 0, ".e01", "EnCase EWF version 1 format", false)},
            { new FileSignature(new byte[] { 0x52, 0x65, 0x63, 0x65, 0x69, 0x76, 0x65, 0x64 }, 0, ".eml", "Email Message var5", false)},
            { new FileSignature(new byte[] { 0x4C, 0x66, 0x4C, 0x65 }, 0, ".evt", "Windows Event Viewer file format", false)},
            { new FileSignature(new byte[] { 0x45, 0x56, 0x46, 0x32 }, 0, ".Ex01", "EnCase EWF version 2 format", false)},
            { new FileSignature(new byte[] { 0x4D, 0x5A }, 0, ".exe", "DOS MZ executable and its descen", false)},
            { new FileSignature(new byte[] { 0x5A, 0x4D }, 0, ".exe", "DOS ZM executable and its descen", false)},
            { new FileSignature(new byte[] { 0x76, 0x2F, 0x31, 0x01 }, 0, ".exr", "OpenEXR image", false)},
            { new FileSignature(new byte[] { 0x45, 0x4D, 0x58, 0x32 }, 0, ".ez2", "Emulator Emaxsynth samples", false)},
            { new FileSignature(new byte[] { 0x45, 0x4D, 0x55, 0x33 }, 0, ".ez3", "Emulator III synth samples", false)},
            { new FileSignature(new byte[] { 0x46, 0x41, 0x58, 0x58 }, 0, ".fax", "", false)},
            { new FileSignature(new byte[] { 0x41, 0x47, 0x44, 0x33 }, 0, ".fh8", "FreeHand 8 document[32][33][34]", false)},
            { new FileSignature(new byte[] { 0x53, 0x49, 0x4D, 0x50, 0x4C, 0x45, 0x20, 0x20 }, 0, ".fits", "Flexible Image Transport System", false)},
            { new FileSignature(new byte[] { 0x66, 0x4C, 0x61, 0x43 }, 0, ".flac", "Free Lossless Audio Codec[25]", false)},
            { new FileSignature(new byte[] { 0x46, 0x4C, 0x49, 0x46 }, 0, ".flif", "Free Lossless Image Format", false)},
            { new FileSignature(new byte[] { 0x46, 0x4C, 0x56 }, 0, ".flv", "Flash Video file", false)},
            { new FileSignature(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, 0, ".gif", "Image file encoded in the Graphi", false)},
            { new FileSignature(new byte[] { 0x47, 0x52, 0x49, 0x42 }, 0, ".grib", "Gridded data(commonly weather o", false)},
            { new FileSignature(new byte[] { 0x50, 0x4D, 0x43, 0x43 }, 0, ".grp", "Windows 3.x Program Manager Prog", false)},
            { new FileSignature(new byte[] { 0x1F, 0x8B }, 0, ".gz", "GZIP compressed file[44]", true)},
            { new FileSignature(new byte[] { 0x4B, 0x43, 0x4D, 0x53 }, 0, ".icm", "ICC profile", false)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0x01, 0x00 }, 0, ".ico", "Computer icon encoded in ICO fil", false)},
            { new FileSignature(new byte[] { 0x5B, 0x5A, 0x6F, 0x6E, 0x65, 0x54, 0x72, 0x61 }, 0, ".Identifier", "Microsoft Zone Identifier for UR", false)},
            { new FileSignature(new byte[] { 0x49, 0x4E, 0x44, 0x58 }, 0, ".idx", "AmiBack Amiga Backup index file", false)},
            { new FileSignature(new byte[] { 0x41, 0x43, 0x42, 0x4D }, 0, ".iff", "", false)},
            { new FileSignature(new byte[] { 0x41, 0x4E, 0x42, 0x4D }, 0, ".iff", "", false)},
            { new FileSignature(new byte[] { 0x41, 0x4E, 0x49, 0x4D }, 0, ".iff", "", false)},
            { new FileSignature(new byte[] { 0x46, 0x54, 0x58, 0x54 }, 0, ".iff", "", false)},
            { new FileSignature(new byte[] { 0x43, 0x44, 0x30, 0x30, 0x31 }, 0, ".iso", "ISO9660 CD / DVD image file[23]", false)},
            { new FileSignature(new byte[] { 0x49, 0x73, 0x5A, 0x21 }, 0, ".isz", "Compressed ISO image", true)},
            { new FileSignature(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 }, 0, ".jpeg", "", false)},
            { new FileSignature(new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }, 0, ".jpg", "JPEG raw or in the JFIF or Exif", false)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x4A, 0x58, 0x4C, 0x20, 0x0D, 0x0A, 0x87, 0x0A }, 0, ".jxl", "Image encoded in the JPEG XL for", false)},
            { new FileSignature(new byte[] { 0x37, 0x48, 0x03, 0x02, 0x00, 0x00, 0x00, 0x00 }, 0, ".kdb", "KDB file", false)},
            { new FileSignature(new byte[] { 0x49, 0x4C, 0x42, 0x4D }, 0, ".lbm", "", false)},
            { new FileSignature(new byte[] { 0xCF, 0x84, 0x01 }, 0, ".lep", "Lepton compressed JPEG image[49]", true)},
            { new FileSignature(new byte[] { 0x1B, 0x4C, 0x75, 0x61 }, 0, ".luac", "Lua bytecode[53]", false)},
            { new FileSignature(new byte[] { 0x4C, 0x5A, 0x49, 0x50 }, 0, ".lz", "lzip compressed file", true)},
            { new FileSignature(new byte[] { 0x04, 0x22, 0x4D, 0x18 }, 0, ".lz4", "LZ4 Frame Format[45]", false)},
            { new FileSignature(new byte[] { 0x62, 0x76, 0x78, 0x32 }, 0, ".lzfse", "LZFSE - Lempel - Ziv style data co", true)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0x01, 0xBA }, 0, ".m2p", "MPEG Program Stream(MPEG - 1 Part", false)},
            { new FileSignature(new byte[] { 0x4D, 0x54, 0x68, 0x64 }, 0, ".mid", "MIDI sound file[26]", false)},
            { new FileSignature(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }, 0, ".mkv", "Matroska media container, includ", false)},
            { new FileSignature(new byte[] { 0x4D, 0x4C, 0x56, 0x49 }, 0, ".MLV", "Magic Lantern Video file[42]", false)},
            { new FileSignature(new byte[] { 0x49, 0x44, 0x33 }, 0, ".mp3", "MP3 file with an ID3v2 container", false)},
            { new FileSignature(new byte[] { 0xFF, 0xFB }, 0, ".mp3", "MPEG - 1 Layer 3 file without an I", false)},
            { new FileSignature(new byte[] { 0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D }, 4, ".mp4", "ISO Base Media file(MPEG - 4)", false)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0x01, 0xB3 }, 0, ".mpg", "MPEG - 1 video and MPEG - 2 video(M", false)},
            { new FileSignature(new byte[] { 0x43, 0x4D, 0x55, 0x53 }, 0, ".mus", "", false)},
            { new FileSignature(new byte[] { 0x4E, 0x45, 0x53, 0x1A }, 0, ".nes", "Nintendo Entertainment System RO", false)},
            { new FileSignature(new byte[] { 0x4E, 0x55, 0x52, 0x55, 0x49, 0x4D, 0x47 }, 0, ".nui", "nuru ASCII / ANSI image and palett", false)},
            { new FileSignature(new byte[] { 0x4E, 0x55, 0x52, 0x55, 0x50, 0x41, 0x4C }, 0, ".nup", "", false)},
            { new FileSignature(new byte[] { 0x4F, 0x67, 0x67, 0x53 }, 0, ".ogg", "Ogg, an open source media contai", false)},
            { new FileSignature(new byte[] { 0x4F, 0x52, 0x43 }, 0, ".orc", "Apache ORC(Optimized Row Column", false)},
            { new FileSignature(new byte[] { 0x65, 0x87, 0x78, 0x56 }, 0, ".p25", "PhotoCap Object Templates", false)},
            { new FileSignature(new byte[] { 0x50, 0x31, 0x0A }, 0, ".pbm", "Portable bitmap", false)},
            { new FileSignature(new byte[] { 0x78, 0x56, 0x34 }, 0, ".pbt", "PhotoCap Template", false)},
            { new FileSignature(new byte[] { 0x0A, 0x0D, 0x0D, 0x0A }, 0, ".pcapng", "PCAP Next Generation Dump File F", false)},
            { new FileSignature(new byte[] { 0x55, 0x55, 0xAA, 0xAA }, 0, ".pcv", "PhotoCap Vector", false)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 11, ".PDB", "PalmPilot Database / Document File", false)},
            { new FileSignature(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }, 0, ".pdf", "PDF document[21]", false)},
            { new FileSignature(new byte[] { 0x50, 0x32, 0x0A }, 0, ".pgm", "Portable Gray Map", false)},
            { new FileSignature(new byte[] { 0x0 }, 0, ".PIC", "IBM Storyboard bitmap file", false)},
            { new FileSignature(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, ".png", "Image encoded in the Portable Ne", false)},
            { new FileSignature(new byte[] { 0x50, 0x33, 0x0A }, 0, ".ppm", "Portable Pixmap", false)},
            { new FileSignature(new byte[] { 0x25, 0x21, 0x50, 0x53 }, 0, ".ps", "PostScript document", false)},
            { new FileSignature(new byte[] { 0x38, 0x42, 0x50, 0x53 }, 0, ".psd", "Photoshop Document file, Adobe P", false)},
            { new FileSignature(new byte[] { 0x21, 0x42, 0x44, 0x4E }, 0, ".pst", "Microsoft Outlook Personal Stora", false)},
            { new FileSignature(new byte[] { 0x51, 0x46, 0x49 }, 0, ".qcow", "qcow file format", false)},
            { new FileSignature(new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 }, 0, ".rar", "Roshal ARchive compressed archiv", true)},
            { new FileSignature(new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 }, 0, ".rar", "Roshal ARchive compressed archiv", true)},
            { new FileSignature(new byte[] { 0x53, 0x45, 0x51, 0x36 }, 0, ".rc", "RCFile columnar file format", false)},
            { new FileSignature(new byte[] { 0xED, 0xAB, 0xEE, 0xDB }, 0, ".rpm", "RedHat Package Manager(RPM) pac", false)},
            { new FileSignature(new byte[] { 0x52, 0x53, 0x56, 0x4B, 0x44, 0x41, 0x54, 0x41 }, 0, ".rs", "QuickZip rs compressed archive[5", true)},
            { new FileSignature(new byte[] { 0x7B, 0x5C, 0x72, 0x74, 0x66, 0x31 }, 0, ".rtf", "Rich Text Format", false)},
            { new FileSignature(new byte[] { 0x3A, 0x29, 0x0A }, 0, ".sml", "Smile file", false)},
            { new FileSignature(new byte[] { 0x53, 0x4D, 0x55, 0x53 }, 0, ".smu", "", false)},
            { new FileSignature(new byte[] { 0x6F, 0x72, 0x6D, 0x61, 0x74, 0x20, 0x33, 0x00 }, 0, ".sqlite", "", false)},
            { new FileSignature(new byte[] { 0x53, 0x51, 0x4C, 0x69, 0x74, 0x65, 0x20, 0x66 }, 0, ".sqlitedb", "SQLite Database[5]", false)},
            { new FileSignature(new byte[] { 0x31, 0x0A, 0x30, 0x30 }, 0, ".srt", "SubRip File", false)},
            { new FileSignature(new byte[] { 0x4D, 0x49, 0x4C, 0x20 }, 0, ".stg", "SEAN : Session Analysis Training", false)},
            { new FileSignature(new byte[] { 0x43, 0x57, 0x53 }, 0, ".swf", "Adobe Flash.swf", false)},
            { new FileSignature(new byte[] { 0x46, 0x57, 0x53 }, 0, ".swf", "Adobe Flash.swf", false)},
            { new FileSignature(new byte[] { 0x75, 0x73, 0x74, 0x61, 0x72, 0x00, 0x30, 0x30 }, 257, ".tar", "tar archive[40]", false)},
            { new FileSignature(new byte[] { 0x00, 0x01, 0x44, 0x54 }, 0, ".TDA", "Palm Desktop Calendar Archive", false)},
            { new FileSignature(new byte[] { 0x20, 0x02, 0x01, 0x62, 0xA0, 0x1E, 0xAB, 0x07 }, 0, ".tde", "Tableau Datasource", false)},
            { new FileSignature(new byte[] { 0x54, 0x44, 0x45, 0x46 }, 0, ".TDEF", "Telegram Desktop Encrypted File", false)},
            { new FileSignature(new byte[] { 0x54, 0x44, 0x46, 0x24 }, 0, ".TDF$", "Telegram Desktop File", false)},
            { new FileSignature(new byte[] { 0x45, 0x52, 0x02, 0x00, 0x00, 0x00 }, 0, ".toast", "Roxio Toast disc image file", false)},
            { new FileSignature(new byte[] { 0x8B, 0x45, 0x52, 0x02, 0x00, 0x00, 0x00 }, 0, ".toast", "Roxio Toast disc image file", false)},
            { new FileSignature(new byte[] { 0x74, 0x6F, 0x78, 0x33 }, 0, ".tox", "Open source portable voxel file[", false)},
            { new FileSignature(new byte[] { 0x47 }, 0, ".ts", "MPEG Transport Stream(MPEG - 2 Pa", false)},
            { new FileSignature(new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00 }, 0, ".ttf", "TrueType font", false)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0xFE, 0xFF }, 0, ".txt", "UTF - 32BE byte order mark for tex", false)},
            { new FileSignature(new byte[] { 0x0E, 0xFE, 0xFF }, 0, ".txt", "SCSU byte order mark for text[19", false)},
            { new FileSignature(new byte[] { 0xEF, 0xBB, 0xBF }, 0, ".txt", "UTF - 8 byte order mark, commonly", false)},
            { new FileSignature(new byte[] { 0xFE, 0xFF }, 0, ".txt", "UTF - 16BE byte order mark, common", false)},
            { new FileSignature(new byte[] { 0xFF, 0xFE }, 0, ".txt", "UTF - 16LE byte order mark, common", false)},
            { new FileSignature(new byte[] { 0xFF, 0xFE, 0x00, 0x00 }, 0, ".txt", "UTF - 32LE byte order mark for tex", false)},
            { new FileSignature(new byte[] { 0x3C, 0x3C, 0x3C, 0x20, 0x4F, 0x72, 0x61, 0x63 }, 0, ".vdi", "VirtualBox Virtual Hard Disk fil", false)},
            { new FileSignature(new byte[] { 0x63, 0x6F, 0x6E, 0x6E, 0x65, 0x63, 0x74, 0x69 }, 0, ".vhd", "Windows Virtual PC Virtual Hard", false)},
            { new FileSignature(new byte[] { 0x76, 0x68, 0x64, 0x78, 0x66, 0x69, 0x6C, 0x65 }, 0, ".vhdx", "Windows Virtual PC Windows 8 Vir", false)},
            { new FileSignature(new byte[] { 0x4B, 0x44, 0x4D }, 0, ".vmdk", "VMDK files[28][29]", false)},
            { new FileSignature(new byte[] { 0x34, 0x12, 0xAA, 0x55 }, 0, ".vpk", "VPK file, used to store game dat", false)},
            { new FileSignature(new byte[] { 0x00, 0x61, 0x73, 0x6D }, 0, ".wasm", "WebAssembly binary format[48]", false)},
            { new FileSignature(new byte[] { 0x4D, 0x53, 0x57, 0x49, 0x4D, 0x00, 0x00, 0x00, 0xD0, 0x00, 0x00, 0x00, 0x00 }, 0, ".wim", "Windows Imaging Format file", false)},
            { new FileSignature(new byte[] { 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C }, 0, ".wma", "", false)},
            { new FileSignature(new byte[] { 0xD7, 0xCD, 0xC6, 0x9A }, 0, ".wmf", "Windows Metafile", false)},
            { new FileSignature(new byte[] { 0x77, 0x4F, 0x46, 0x46 }, 0, ".woff", "WOFF File Format 1.0", false)},
            { new FileSignature(new byte[] { 0x77, 0x4F, 0x46, 0x32 }, 0, ".woff2", "WOFF File Format 2.0", false)},
            { new FileSignature(new byte[] { 0x78, 0x61, 0x72, 0x21 }, 0, ".xar", "eXtensible ARchive format[35]", false)},
            { new FileSignature(new byte[] { 0x67, 0x69, 0x6D, 0x70, 0x20, 0x78, 0x63, 0x66 }, 0, ".xcf", "XCF(file format)", false)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0x00, 0x3C, 0x00, 0x00, 0x00, 0x3F }, 0, ".xml", "eXtensible Markup Language[17][4", false)},
            { new FileSignature(new byte[] { 0x00, 0x3C, 0x00, 0x3F, 0x00, 0x78, 0x00, 0x6D }, 0, ".xml", "eXtensible Markup Language[17][4", false)},
            { new FileSignature(new byte[] { 0x3C, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00 }, 0, ".xml", "eXtensible Markup Language[17][4", false)},
            { new FileSignature(new byte[] { 0x3C, 0x00, 0x3F, 0x00, 0x78, 0x00, 0x6D, 0x00 }, 0, ".xml", "eXtensible Markup Language[17][4", false)},
            { new FileSignature(new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C, 0x20 }, 0, ".xml", "eXtensible Markup Language[17][4", false)},
            { new FileSignature(new byte[] { 0x4C, 0x6F, 0xA7, 0x94, 0x93, 0x40 }, 0, ".xml", "eXtensible Markup Language[17][4", false)},
            { new FileSignature(new byte[] { 0x2F, 0x2A, 0x20, 0x58, 0x50, 0x4D, 0x20, 0x2A }, 0, ".xpm", "X PixMap", false)},
            { new FileSignature(new byte[] { 0xFD, 0x37, 0x7A, 0x58, 0x5A, 0x00 }, 0, ".xz", "XZ compression utility", true)},
            { new FileSignature(new byte[] { 0x59, 0x55, 0x56, 0x4E }, 0, ".yuv", "", false)},
            { new FileSignature(new byte[] { 0x1F, 0x9D }, 0, ".z", "compressed file(often tar zip)", true)},
            { new FileSignature(new byte[] { 0x1F, 0xA0 }, 0, ".z", "Compressed file(often tar zip)", true)},
            { new FileSignature(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 0, ".zip", "zip file format and formats base", false)},
            { new FileSignature(new byte[] { 0x78, 0x01 }, 0, ".zlib", "No Compression(no preset dictio", false)},
            { new FileSignature(new byte[] { 0x78, 0x20 }, 0, ".zlib", "No Compression(with preset dict", false)},
            { new FileSignature(new byte[] { 0x78, 0x5E }, 0, ".zlib", "Best speed(no preset dictionary", true)},
            { new FileSignature(new byte[] { 0x78, 0x7D }, 0, ".zlib", "Best speed(with preset dictiona", true)},
            { new FileSignature(new byte[] { 0x78, 0x9C }, 0, ".zlib", "Default Compression(no preset d", true)},
            { new FileSignature(new byte[] { 0x78, 0xBB }, 0, ".zlib", "Default Compression(with preset", true)},
            { new FileSignature(new byte[] { 0x78, 0xDA }, 0, ".zlib", "Best Compression(no preset dict", true)},
            { new FileSignature(new byte[] { 0x78, 0xF9 }, 0, ".zlib", "Best Compression(with preset di", true)},
            { new FileSignature(new byte[] { 0x5A, 0x4F, 0x4F }, 0, ".zoo", "Zoo(file format)", false)},
            { new FileSignature(new byte[] { 0x28, 0xB5, 0x2F, 0xFD }, 0, ".zst", "Zstandard compressed file[57][58", true)},
            { new FileSignature(new byte[] { 0x78 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0x00, 0x6C, 0x00, 0x00, 0x00, 0x20 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x00, 0x00, 0x00, 0x78, 0x00, 0x00, 0x00, 0x6D }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x00, 0x01, 0x00, 0x00 }, 0, ".xxx", "Palm Desktop Data File(Access f", false)},
            { new FileSignature(new byte[] { 0x00, 0x6C, 0x00, 0x20 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x02, 0x00, 0x00, 0x00 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x05, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x06, 0x07, 0xE1, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x54 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x20, 0x44, 0x69, 0x73, 0x6B, 0x20, 0x49, 0x6D }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x23, 0x21 }, 0, ".xxx", "Script or data to be passed to t", false)},
            { new FileSignature(new byte[] { 0x24, 0x53, 0x44, 0x49, 0x30, 0x30, 0x30, 0x31 }, 0, ".xxx", "System Deployment Image, a disk", false)},
            { new FileSignature(new byte[] { 0x27, 0x05, 0x19, 0x56 }, 0, ".xxx", "U - Boot / uImage.Das U - Boot Univ", false)},
            { new FileSignature(new byte[] { 0x2B, 0x2F, 0x76, 0x2B }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x2B, 0x2F, 0x76, 0x2F }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x2B, 0x2F, 0x76, 0x38 }, 0, ".xxx", "UTF - 7 byte order mark for text[1", false)},
            { new FileSignature(new byte[] { 0x2B, 0x2F, 0x76, 0x39 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x2F }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x3A }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x3D, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x41, 0x43, 0x4F, 0x4E }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x41, 0x56, 0x49, 0x20 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x43, 0x44, 0x44, 0x41 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x43, 0x52 }, 0, ".xxx", "Canons RAW format is based on T", false)},
            { new FileSignature(new byte[] { 0x44, 0x43, 0x4D, 0x01, 0x50, 0x41, 0x33, 0x30 }, 0, ".xxx", "Windows Update Binary Delta Comp", true) },
            { new FileSignature(new byte[] { 0x44, 0x49, 0x53, 0x4B }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x46, 0x41, 0x4E, 0x54 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x49, 0x46, 0x00, 0x01 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x4A, 0x6F, 0x79, 0x21 }, 0, ".xxx", "Preferred Executable Format", false)},
            { new FileSignature(new byte[] { 0x50, 0x41, 0x52, 0x31 }, 0, ".xxx", "Apache Parquet columnar file for", false)},
            { new FileSignature(new byte[] { 0x51, 0x4C, 0x43, 0x4D }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x52, 0x4E, 0x43, 0x01 }, 0, ".xxx", "Compressed file using Rob Northe", true)},
            { new FileSignature(new byte[] { 0x52, 0x4E, 0x43, 0x02 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x54, 0x41, 0x50, 0x45 }, 0, ".xxx", "Microsoft Tape Format", false)},
            { new FileSignature(new byte[] { 0x57, 0x41, 0x56, 0x45 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x57, 0x45, 0x42, 0x50 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x58, 0x35, 0x30, 0x39, 0x4B, 0x45, 0x59 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x60, 0x00, 0x00, 0x00 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x61, 0x67, 0x65, 0x20, 0x3E, 0x3E, 0x3E }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x69, 0x66, 0x00, 0x00 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x6C, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x6C, 0x00, 0x20 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x6C, 0x65, 0x20, 0x56, 0x4D, 0x20, 0x56, 0x69 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x6D, 0x61, 0x72, 0x6B, 0x00, 0x00, 0x00, 0x00 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x6E, 0x73, 0x66, 0x65, 0x72, 0x5D }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x72, 0x74, 0x75, 0x61, 0x6C, 0x42, 0x6F, 0x78 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x75, 0x73, 0x74, 0x61, 0x72, 0x20, 0x20, 0x00 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x78, 0x00, 0x00, 0x00, 0x6D, 0x00, 0x00, 0x00 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x7F, 0x45, 0x4C, 0x46 }, 0, ".xxx", "Executable and Linkable Format", false)},
            { new FileSignature(new byte[] { 0xCE, 0xFA, 0xED, 0xFE }, 0, ".xxx", "Mach - O binary(reverse byte orde", false)},
            { new FileSignature(new byte[] { 0xCF, 0xFA, 0xED, 0xFE }, 0, ".xxx", "Mach - O binary(reverse byte orde", false)},
            { new FileSignature(new byte[] { 0xDD, 0x73, 0x66, 0x73 }, 0, ".xxx", "UTF - EBCDIC byte order mark for t", false)},
            { new FileSignature(new byte[] { 0xFE, 0xED, 0xFA, 0xCE }, 0, ".xxx", "Mach - O binary(32 - bit)", false)},
            { new FileSignature(new byte[] { 0xFE, 0xED, 0xFA, 0xCF }, 0, ".xxx", "Mach - O binary(64 - bit)", false)},
            { new FileSignature(new byte[] { 0xFE, 0xED, 0xFE, 0xED }, 0, ".xxx", "JKS JavakeyStore", false)},
            { new FileSignature(new byte[] { 0xFF, 0xD8, 0xFF, 0xEE }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0xFF, 0xF2 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0xFF, 0xF3 }, 0, ".xxx", "", false)},
            { new FileSignature(new byte[] { 0x18, 0x95 }, 0, ".eos", "EOS Location Controller File", true)}
        };

        //TODO: this has not been tested
        /// <summary>
        /// Gets the <see cref="FileSignature"/> from <see cref="FileInfo"/>
        /// </summary>
        /// <param name="file"><see cref="FileInfo"/> to get info for</param>
        /// <returns>List of <see cref="FileSignature"/> that match file type</returns>
        public static IEnumerable<FileSignature> GetFileSignatureFromExtension(this FileInfo file)
        {
            return GetFileSignatureFromExtension(file.Extension);
        }

        //TODO: this has not been tested
        /// <summary>
        /// Gets the <see cref="FileSignature"/> from string
        /// </summary>
        /// <param name="fileExtension">string to get info for</param>
        /// <returns>List of <see cref="FileSignature"/> that match extension type</returns>
        public static IEnumerable<FileSignature> GetFileSignatureFromExtension(this string fileExtension)
        {
            return FileSignatures.Where(f => f.Extension == fileExtension);
        }

        /// <summary>
        /// Gets the <see cref="FileSignature"/> info from Magic Header byte array
        /// </summary>
        /// <param name="bytes">array representing Magic Header</param>
        /// <returns><see cref="FileSignature"/> info that matches Magic Header bytes</returns>
        public static FileSignature GetFileSignatureFromMagicHeader(this byte[] bytes)
        {
            return FileSignatures.Where(f => f.MagicHeader.SequenceEqual(bytes.Skip(f.Offset).Take(f.MagicHeader.Length))).FirstOrDefault();

        }
    }
}
