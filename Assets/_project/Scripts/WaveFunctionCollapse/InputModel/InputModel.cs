////////////////////////////////////////////////////////////
// File: InputModel
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 21/10/21
// Brief: The script which holds information about the input model and can generate the patterns
//////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    [Serializable]
    public class InputModel
    {
        #region Variables

        public Grid Model;
        private bool includeFlipping;
        
        #endregion

        #region Public Methods

        public InputModel(Grid modelGrid, bool flipping)
        {
            Model = modelGrid;
            includeFlipping = flipping;
        }

        //This essentially records each cells' neighbours and makes patterns out of them (N being the size of the chunks)
        public List<Pattern> GeneratePatterns(int N)
        {
            int chunks_x = Model.width / N;
            int chunks_y = Model.height / N;
            List<Pattern> patterns = new List<Pattern>();

            for (int i = 0; i < chunks_y; ++i)
            {
                for(int j = 0; j < chunks_x; ++j)
                {
                    // Normal orientation
                    Pattern pattern = new Pattern();

                    int start_x = j * N;
                    int end_x = (j + 1) * N;
                    int start_y = i * N;
                    int end_y = (i + 1) * N;

                    for (int y = start_y; y < end_y; ++y)
                    {
                        for(int x = start_x; x < end_x; ++x)
                        {
                            Cell idx = Model.GridCells[x, y];
                            pattern.AddTile(idx.tileUsed);
                        }
                    }

                    patterns.Add(pattern);

                    if (includeFlipping)
                    {
                        // Flip horizontal
                        pattern = new Pattern();

                        for(int y = start_y; y < end_y; ++y)
                        {
                            for(int x = start_x; x < end_x; ++x)
                            {
                                Cell idx = Model.GridCells[end_x - (x + 1), y];
                                pattern.AddTile(idx.tileUsed);
                            }
                        }

                        patterns.Add(pattern);

                        // Flip vertical
                        pattern = new Pattern();

                        for(int y = start_y; y < end_y; ++y)
                        {
                            for(int x = start_x; x < end_x; ++x)
                            {
                                Cell idx = Model.GridCells[x, end_y - (y + 1)];
                                pattern.AddTile(idx.tileUsed);
                            }
                        }

                        patterns.Add(pattern);

                        // Flip both
                        pattern = new Pattern();

                        for(int y = start_y; y < end_y; ++y)
                        {
                            for(int x = start_x; x < end_x; ++x)
                            {
                                Cell idx = Model.GridCells[end_x - (x + 1), end_y - (y + 1)];
                                pattern.AddTile(idx.tileUsed);
                            }
                        }

                        patterns.Add(pattern);
                    }
                }
            }

           return patterns;
        }

        #endregion

        #region Private Methods
        #endregion
    }

}