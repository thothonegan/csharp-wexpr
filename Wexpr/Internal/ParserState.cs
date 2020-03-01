using System;
using System.Collections.Generic;
using System.Text;

namespace Wexpr.Internal
{
    //
    /// <summary>
    /// Manages the state of the parser while parsing.
    /// </summary>
    //
    internal class ParserState
    {
        public ParserState()
        {
            InternalReferenceMap = new ReferenceTable(); // used for storing our refs

            // starting position
            Line = 1;
            Column = 1;
        }

        public void MoveForwardBasedOnString (string str)
        {
            for (int i=0; i < str.Length; ++i)
            {
                if (str[i] == '\n') // newline
                {
                    Line += 1;
                    Column = 1;
                }
                else // normal
                {
                    Column += 1;
                }
            }
        }

        // position in the data we loaded
        public int Line;
        public int Column;

        // reference information lists
        public ReferenceTable InternalReferenceMap; // The internal one within the file. Takes priority.
        public ReferenceTable ExternalReferenceMap; // The external one for lookups, if provied.
    }
}
