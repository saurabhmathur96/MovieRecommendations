using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace MovieRecommendations.Data
{
    public class MovieList : IMovieList
    {
        public List<Movie> movies;

        public MovieList() 
        {
            this.movies = new List<Movie>();
        }

        public void ReadFromFile(string fileName) 
        {
            string text = System.IO.File.ReadAllText(fileName);
            var list = JsonConvert.DeserializeObject<MovieList>(text);
            this.movies = list.movies;
        }
        public List<Movie> FindAll(string q) 
        {
            if (this.movies.Count == 0) {
                ReadFromFile("movies.json");
            }
            return this.movies.FindAll((movie) => movie.Title.ToLower().Contains(q.ToLower()));
        }

        public Movie FindById(int id) 
        {
            if (this.movies.Count == 0) {
                ReadFromFile("movies.json");
            }

            return this.movies.Find((movie) => movie.MovieId == id );
        }

        public List<Movie> FindById(List<int> ids) 
        {
            if (this.movies.Count == 0) {
                ReadFromFile("movies.json");
            }

            return this.movies.FindAll((movie) => ids.Contains(movie.MovieId) );
        }
    }
}