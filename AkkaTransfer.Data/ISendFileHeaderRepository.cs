﻿using AkkaTransfer.Common;

namespace AkkaTransfer.Data
{
    internal interface ISendFileHeaderRepository
    {
        void AddFileHeaderAndPieces(FilePartMessage filePartMessage);
        void DeleteAll();
        SendFileHeader GetFileHeaderByFilename(string filename);
        SendFilePiece[] GetFilePiecesByFilenameAndPosition(string filename, int[] positions);
    }
}