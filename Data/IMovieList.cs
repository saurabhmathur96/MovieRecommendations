using System.Collections.Generic;

namespace MovieRecommendations.Data
{
    public interface IMovieList
    {
         void ReadFromFile(string fileName);
         List<Movie> FindAll(string q);
         Movie FindById(int id);
         List<Movie> FindById(List<int> ids);
    }
}