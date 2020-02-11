using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;


namespace MovieLibrary
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            //variable for console readline
            string choice;

            //data file name along with path on windows machine - for testing, use local file and set path
            string theDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string file = theDirectory + "movies.csv";

            Console.WriteLine(file);

            //create a new DataTable to hold movie info
            DataTable movieData = new DataTable();

            //initialize variable to hold max value
            int maxMovieID = int.MinValue;

            do
            {
                //menu - Note this construct allows only one reading of file
                //given the dataset is declared outside of Do While Loop
                //attempting to read the file again may cause data corruption in memory
                //and possibly the storage file

                Console.WriteLine("1) Read data from file.");
                Console.WriteLine("2) Add data to file.");
                Console.WriteLine("Enter any other key to exit.");

                //input response
                choice = Console.ReadLine();

                if (choice == "1")
                {

                    logger.Info("Program started");


                    if (!File.Exists(file))
                    {
                        logger.Error("File does not exist: {File}", file);
                        Console.WriteLine("File does not exist.");
                    }
                    else
                    {
                        try
                        {
                            //read data from file
                            StreamReader sr = new StreamReader(file);

                            //read header from first line of an file
                            string[] headers = sr.ReadLine().Split(',');

                            //add columns to DataTable
                            foreach (string header in headers)
                            {
                                movieData.Columns.Add(header);
                            }

                            while (!sr.EndOfStream)
                            {
                                // lines 80-91 was completed with the input of Matt Green
                                // reads next line after header line and data cleanse
                                // find within quotes any commas and replace with tilde~
                                // parse remaining string on commas
                                // within each array element (substring), replace the tilde~ with comma
                                // replace quote with space
                                // trim the space
                                string line = sr.ReadLine();
                                var regex = new Regex("\\\"(.*?)\\\"");
                                var output = regex.Replace(line, m => m.Value.Replace(',', '~'));
                                string[] rows = output.Split(',');

                                // Once file is read with proper parse, replace ~ 
                                for (int i = 0; i < rows.Length; i++)
                                {
                                    rows[i] = regex.Replace(rows[i], m => m.Value.Replace('~', ','));
                                    rows[i] = regex.Replace(rows[i], m => m.Value.Replace('"', ' '));
                                    rows[i] = rows[i].Trim();
                                }

                                //add a new row to datatable
                                DataRow movieDataRow = movieData.NewRow();

                                //add row elements to datarow
                                for (int i = 0; i < headers.Length; i++)
                                {
                                    movieDataRow[i] = rows[i];
                                }
                                movieData.Rows.Add(movieDataRow);

                            }

                            foreach (DataRow dataRow in movieData.Rows)
                            {

                                //identifies the max movieId in datatable column for menu option 2
                                //this is better than row count since original file may not follow increments of 1
                                string movieID = dataRow.Field<string>("movieId");
                                maxMovieID = Math.Max(maxMovieID, Convert.ToInt32(movieID));

                                // display each row item in sequence as a visual check
                                foreach (var item in dataRow.ItemArray)
                                {
                                    Console.WriteLine(item);
                                }

                            }

                            //displays the max movieID in datatable as a visual check
                            Console.WriteLine("Max Movie ID {0}", maxMovieID);
                            Console.WriteLine("Count of Movie Records {0}", movieData.Rows.Count);

                            sr.Close();
                        }

                        catch (Exception ex)
                        {
                            logger.Error(ex.Message);
                        }

                        logger.Info("Movie Count {0}", movieData.Rows.Count);
                    }
                }
                else if (choice == "2")

                {
                    //User inputs a new movie title
                    Console.WriteLine("Enter a Movie Title : ");
                    string newMovie = Console.ReadLine();


                    //simple check for duplicate title
                    Boolean movieExists = false;

                    foreach (DataRow dataRow in movieData.Rows)
                    {

                        //reads the title field of the datarow
                        string name = dataRow.Field<string>("title");

                        if (newMovie.ToLower() == name.ToLower())
                        {
                            Console.WriteLine("The movie exists!  Movie not added.");
                        //if movie title is found then change boolean to true
                            movieExists = true;

                        }
                    }

                    //if movie is new, get movie info and save the file

                    if (movieExists == false)
                    {
                        Console.WriteLine("The movie is new!");

                        //User inputs a new movie's genre
                        Console.WriteLine("Enter the genre : ");
                        string genre = Console.ReadLine();

                        //add a new row to datatable
                        DataRow movieDataRow = movieData.NewRow();

                        //add row elements to datarow

                        movieDataRow[0] = ++maxMovieID;  // movie id increment
                        movieDataRow[1] = newMovie;  // movie title
                        movieDataRow[2] = genre;  // movie genre

                        movieData.Rows.Add(movieDataRow);

                        //write to file with try catch NLog with each movie add

                        try
                        {
                            StreamWriter sw = new StreamWriter(file);
                            String header = "";

                            foreach (DataColumn column in movieData.Columns)
                            {
                                header = header + column.ColumnName + ",";
                            }

                            //remove extra comma delimiter
                            header = header.Substring(0, header.Length - 1);

                            sw.WriteLine(header);
                            Console.WriteLine(header);

                            foreach (DataRow dataRow in movieData.Rows)
                            {
                                sw.WriteLine($"{dataRow.ItemArray[0]},{dataRow.ItemArray[1]},{dataRow.ItemArray[2]}");
                                Console.WriteLine($"{dataRow.ItemArray[0]},{dataRow.ItemArray[1]},{dataRow.ItemArray[2]}");

                            }

                            Console.WriteLine("Movie title has been added and data written to file");
                            Console.WriteLine("Move count is {0}", movieData.Rows.Count);
                            sw.Close();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.Message);
                        }

                    }
                }
                else
                {
                    Console.WriteLine("The End.");
                    logger.Info("Program ended");
                }
            } while (choice == "1" || choice == "2");

        }
    }
}

