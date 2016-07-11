using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

using System.Text;
using MovieRecommendations.Data;
using MovieRecommendations.Models;

namespace MovieRecommendations.Controllers
{
    [Authorize]
    public class MoviesController : Controller 
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMovieList _movies;
        private readonly ApplicationDbContext _dbContext;

        public MoviesController(UserManager<ApplicationUser> userManager, IMovieList movies, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _movies = movies;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Index() 
        {
            return View();
        }


        [HttpGet]
        public List<Movie> Find(string q)
        {
            if (q == null) 
            {
                q = "";
            }
            var result = new List<Movie>(_movies.FindAll(q));
            return new List<Movie>(result.Take(Math.Min(result.Count, 10)));
        }

        [HttpGet]
        public async Task<List<Movie>>  Similar(string id)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {

                    Inputs = new Dictionary<string, StringTable> () { 
                        { 
                            "input1", 
                            new StringTable() 
                            {
                                ColumnNames = new string[] {"movieId"},
                                Values = new string[,] {  { id }  }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>() {}
                };
                const string apiKey = "Yatk6tAulsejz0dX5PNW/ai052O1r/1lkTHQBCnXByy2yuwUllnA/pnRbz8sts9L8knE1c2ST5tYiC7Gux4J4g=="; 
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", apiKey);

                client.BaseAddress = new Uri("https://asiasoutheast.services.azureml.net/workspaces/81a195ecaa21421d87532f7daad3f4fb/services/13c1c10665ba409c84a42cffed26583b/execute?api-version=2.0&details=true");
                
                HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(scoreRequest), Encoding.UTF8, "application/json");
                Console.WriteLine(JsonConvert.SerializeObject(scoreRequest));
                HttpResponseMessage response = await client.PostAsync("", contentPost);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Result: {0}", result);
                    
                    var json = JsonConvert.DeserializeObject<Dictionary<string, Results>>(result);
                    
                    var movieIds = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(json["Results"].output1.value_.Values[0, 0])["movieIds"];
                    
                    return _movies.FindById(movieIds);

                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }

                var empty = new List<Movie>();
                return empty;
            }

        }
        
        [HttpGet]
        public List<UserRating> GetAllRatings() 
        {
            var userName = _userManager.GetUserName(HttpContext.User);
            return this._dbContext.ratings.Where(rating => rating.UserName == userName).ToList();
        }

        [HttpGet]
        public IActionResult Rate() 
        {
            return View();
        }

        [HttpPost]
        public async Task<UserRating> Rate([FromForm]int movieId, [FromForm]int rating) 
        {
            Console.WriteLine(movieId);
            
            var user = await GetCurrentUserAsync();
            var userRating = _dbContext.ratings.SingleOrDefault(r => (r.UserName.Equals(user.UserName)) && (r.MovieId == movieId));
            Console.WriteLine(userRating);
            if (userRating == null)
            {
                userRating = new UserRating();
                userRating.MovieId = movieId;
                userRating.Rating = rating;
                userRating.UserName = user.UserName;
                _dbContext.ratings.Add(userRating);
            }
            else
            {
                userRating.Rating = rating;
                _dbContext.ratings.Update(userRating);
            }
            Console.WriteLine(userRating.MovieId);
            Console.WriteLine(userRating.UserName);
            Console.WriteLine(userRating.Rating);
            _dbContext.SaveChanges();

            return userRating;
            
        }


        [HttpGet]
        [Authorize]
        public async Task<List<PredictedRatingMovie>> GetRecommendations()
        {
            var userRatings = GetAllRatings();

            using (var client = new HttpClient())
            {
                var d = new Dictionary<string, List<UserRating>>();
                d.Add("Ratings", userRatings);

                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, StringTable> () { 
                        { 
                            "input1", 
                            new StringTable() 
                            {
                                ColumnNames = new string[] {"ratings", "n"},
                                
                                Values = new string[,] {  { JsonConvert.SerializeObject(d), "50" }  }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>() {}
                };
                const string apiKey = "ay5exmv8DzrCqofhjXG9yphvGGzMXWAsM2yq+vJ/7SobjqoSYDZ+wUYpDFFvGC62BaOTZggb95C5115QimYTrw=="; 
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", apiKey);

                client.BaseAddress = new Uri("https://asiasoutheast.services.azureml.net/workspaces/81a195ecaa21421d87532f7daad3f4fb/services/8334f3e3d0f8417cbf23fa67a0a1be3f/execute?api-version=2.0&details=true");

                HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(scoreRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("", contentPost);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Result: {0}", result);
                    var json = JsonConvert.DeserializeObject<Dictionary<string, Results>>(result);
                    
                    var predictedRatings = JsonConvert.DeserializeObject<Dictionary<string, List<PredictedRating>>>(json["Results"].output1.value_.Values[0, 0])["PredictedRatings"];
                    var predictedRatingMovies = new List<PredictedRatingMovie>();
                    foreach (var r in predictedRatings)
                    {
                        var movie = _movies.FindById((int)r.MovieId);
                        if (movie == null)
                        {
                            continue;
                        }
                        predictedRatingMovies.Add(new PredictedRatingMovie() 
                        {
                            MovieId = movie.MovieId, 
                            Rating = r.Rating,
                            Overview = movie.Overview,
                            Title = movie.Title,
                            PosterPath = movie.PosterPath 
                        });
                    }
                    return predictedRatingMovies;
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                    
                }

                var empty = new List<PredictedRatingMovie>();
                return empty;
                
            }
        }


        [HttpGet]
        public IActionResult Recommendations() 
        {
            return View();
        }

        
        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(HttpContext.User);
        }

        public class PredictedRating
        {
            public double MovieId {get; set;}

            [JsonProperty("PredictedRating")]
            public double Rating {get; set;}
        }

    }

}