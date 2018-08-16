using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace ToastyNetworksUpdateAnnouncer
{
    public class UpdateChecker
    {
        private DiscordBot Bot { get; set; }
        public string ModpackName { get; private set; }
        public string ChangelogUrl { get; private set; }
        public static List<int> AnnouncedModpackIdList { get; private set; }

        public UpdateChecker(DiscordBot program)
        {
            Bot = program;
        }

        public void CreateConnection()
        {
            Modpack modpack = new Modpack();
            try
            {
                foreach (int modpackId in modpack.ModpackIds)
                {
                    try
                    {
                        string url = "http://curse.nikky.moe/api/addon/" + modpackId + "/files";
                        string json = GetModpackContent(url);
                        dynamic modpackData = JsonConvert.DeserializeObject(json);
                        dynamic modpackMetaData = GetModpackMeta(modpackId);
                        ModpackName = modpackMetaData["name"].ToString();
                        ChangelogUrl = modpackMetaData["webSiteURL"].ToString() + "/changes";
                        foreach (var modpackFile in modpackData)
                        {
                            Modpack modpk = new Modpack(ModpackName, modpackId, modpackFile["fileName"].ToString(),
                                modpackFile["releaseType"].ToString(), ChangelogUrl);
                            modpack.ModpackList.Add(modpk);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Something went wrong trying to get the modpack data from: " + modpackId);
                    }
                }

                CheckForReleaseTypeAsync(modpack.ModpackList);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public dynamic GetModpackMeta(int modpackId)
        {
            try
            {
                string url = "http://curse.nikky.moe/api/addon/" + modpackId;
                string json = GetModpackContent(url);
                dynamic modpackMetaData = JsonConvert.DeserializeObject(json);
                return modpackMetaData;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string GetModpackContent(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
                request.Method = WebRequestMethods.Http.Get;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string output = reader.ReadToEnd();
                response.Close();
                return output;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async void CheckForReleaseTypeAsync(List<Modpack> modpackList)
        {
            try
            {
                foreach (Modpack modpack in modpackList)
                {
                    if (modpack.VersionType == "RELEASE")
                    {
                        if (DoesModpackExistInDatabase(modpack))
                        {
                            if (DoesModpackWithSameVersionExistInDatabase(modpack))
                            {
                            }
                            else
                            {
                                if (!HasModpackBeenUpdatedInPast(modpack))
                                {
                                    AddModpackToDatabase(modpack, true);
                                    AnnouncedModpackIdList.Add(modpack.Id);
                                    Console.WriteLine("Announcement should now be sent to the Discord server");
                                    await CreateAnnouncement(modpack);
                                }
                                else
                                {
                                    Console.WriteLine("FTB Messing up again, modpack has already been announced so it was skipped");
                                }
                            }
                        }
                        else
                        {
                            AddModpackToDatabase(modpack, false);
                            Console.WriteLine("No announcement as modpack is new!");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task CreateAnnouncement(Modpack modpack)
        {
            await Bot.Announce(modpack.Name, modpack.Version, modpack.ChangelogUrl, true, 225347404197658624);
            await Bot.Announce(modpack.Name, modpack.Version, modpack.ChangelogUrl, false, 241556187752038401);
        }

        private bool DoesModpackExistInDatabase(Modpack modpack)
        {
            try
            {
                using (MySqlConnection connection = Database.GetConnectionString())
                {
                    connection.Open();

                    using (MySqlCommand selectId =
                        new MySqlCommand("SELECT IFNULL(MAX(id), 0) FROM modpackupdates WHERE (id) = (@id)",
                            connection))
                    {
                        selectId.Parameters.AddWithValue("id", modpack.Id);
                        if (Convert.ToInt32(selectId.ExecuteScalar()) == 0)
                        {
                            return false;
                        }

                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private bool DoesModpackWithSameVersionExistInDatabase(Modpack modpack)
        {
            try
            {
                using (MySqlConnection connection = Database.GetConnectionString())
                {
                    connection.Open();

                    using (MySqlCommand selectId =
                        new MySqlCommand("SELECT IFNULL(MAX(id), 0) FROM modpackupdates WHERE (version) = (@version)",
                            connection))
                    {
                        selectId.Parameters.AddWithValue("version", modpack.Version);
                        if (Convert.ToInt32(selectId.ExecuteScalar()) == 0)
                        {
                            return false;
                        }

                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private bool HasModpackBeenUpdatedInPast(Modpack modpack)
        {
            try
            {
                if (AnnouncedModpackIdList.Contains(modpack.Id))
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void AddModpackToDatabase(Modpack modpack, bool removePreviousEntry)
        {
            try
            {
                using (MySqlConnection connection = Database.GetConnectionString())
                {
                    connection.Open();
                    if (removePreviousEntry)
                    {
                        using (MySqlCommand removeModpack =
                            new MySqlCommand("DELETE FROM modpackupdates WHERE (id) = (@id)", connection))
                        {
                            removeModpack.Parameters.AddWithValue("id", modpack.Id);
                            removeModpack.ExecuteNonQuery();
                        }
                    }

                    using (MySqlCommand insertModpack =
                        new MySqlCommand(
                            "INSERT INTO modpackupdates (id, version, name, changelogurl) VALUES (@id, @version, @name, @changelogurl)",
                            connection))
                    {
                        insertModpack.Parameters.AddWithValue("id", modpack.Id);
                        insertModpack.Parameters.AddWithValue("version", modpack.Version);
                        insertModpack.Parameters.AddWithValue("name", modpack.Name);
                        insertModpack.Parameters.AddWithValue("changelogUrl", modpack.ChangelogUrl);
                        Console.WriteLine(modpack.Id + " " + modpack.Version + " was added to the database");
                        insertModpack.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}