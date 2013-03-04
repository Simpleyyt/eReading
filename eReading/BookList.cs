using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace eReading
{
    public class BookList
    {
        private ArrayList _booklist = new ArrayList();
        public bool GetMoreable { get; set; }
        public BookInfo this[int index]
        {
            get
            {
                return (BookInfo)_booklist[index];
            }
        }
        public int Count
        {
            get 
            {
                return _booklist.Count; 
            }
        }
        public void addBook(BookInfo book)
        {
            _booklist.Add(book);
        }
        public BookInfo findBookAt(int index)
        {
            return (BookInfo)_booklist[index];
        }
        public void deleteAll()
        {
            _booklist.RemoveRange(0, _booklist.Count);
        }
    }
}
