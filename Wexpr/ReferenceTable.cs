using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wexpr
{
    //
    /// <summary>
    /// A table of expressions given names
    /// </summary>
    /// 
    /// Stores a list of expressions with a given name, allowing you to pull them out.
    /// Generally used as a list of references, which allows '*[asdf]' in wexpr to pull out
    /// an expression.
    //
    public sealed class ReferenceTable
    {
        // --- public Construction/Destruction

        //
        /// <summary>
        /// Creates an empty reference table
        /// </summary>
        //
        public ReferenceTable ()
        {}

        // --- public Keys/Values

        //
        /// <summary>
        ///  Set the expression for the given key
        /// </summary>
        /// <param name="key">The key to assign to</param>
        /// <param name="expression">The expression to assign it.</param>
        //
        public void SetExpressionForKey(string key, Expression expression)
        {
            m_hash[key] = expression;
        }

        //
        /// <summary>
        /// Return the expression for the given key if found.
        /// </summary>
        /// <param name="key">The key to fetch.</param>
        /// <returns>The expression found, or null if not.</returns>
        //
        public Expression ExpressionForKey(string key)
        {
            if (!m_hash.ContainsKey(key))
                return null;

            return m_hash[key];
        }

        //
        /// <summary>
        ///  Remove a key from the reference table.
        /// </summary>
        /// <param name="key">The key to remove</param>
        //
        public void RemoveKey(string key)
        {
            m_hash.Remove(key);
        }

        //
        /// <summary>
        /// The number of keys in the table
        /// </summary>
        //
        public int Count
        { get { return m_hash.Count;  } }

        //
        /// <summary>
        /// Return the index of the given key.
        /// </summary>
        /// <param name="key">The key to lookup</param>
        /// <returns>The index the key was found, or Count if not found.</returns>
        //
        public int IndexOfKey (string key)
        {
            var keys = m_hash.Keys;
            for (var i=0; i < keys.Count; ++i)
            {
                if (keys.ElementAt(i) == key)
                    return i;
            }

            return Count;
        }

        //
        /// <summary>
        /// Get the key at the given index in the table.
        /// </summary>
        /// <param name="index">The index in the table.</param>
        /// <returns>The key at the given index, or null if invalid index.</returns>
        //
        public string KeyAtIndex (int index)
        {
            return m_hash.Keys.ElementAt(index);
        }
        
        //
        /// <summary>
        /// Return the expression at the given index.
        /// </summary>
        /// <param name="index">The index in the table.</param>
        /// <returns>The expression at the given index, or null if invalid index.</returns>
        //
        public Expression ExpressionAtIndex (int index)
        {
            return m_hash.Values.ElementAt(index);
        }

        // --- private members

        //
        /// <summary>
        /// The hash for the reference table.
        /// </summary>
        //
        private Dictionary<string, Expression> m_hash = new Dictionary<string, Expression>();
    }
}
