using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        //Key of the dictionary :username , value: list pf connection IDs 
        private static readonly Dictionary<string,List<string>> OnlineUsers = 
            new Dictionary<string, List<string>>();
        
        // To Track every connections id as every time they log in , they would have a different id
        public Task UserConnected(string username,string connectionId){
            lock(OnlineUsers)
            {
                if(OnlineUsers.ContainsKey(username)){
                    OnlineUsers[username].Add(connectionId);
                }
                else{
                    OnlineUsers.Add(username,new List<string>{connectionId});
                }
            }
            return Task.CompletedTask;
        }
        public Task UserDisconnected(string username,string connectionId){
            lock(OnlineUsers){
                if(!OnlineUsers.ContainsKey(username)) return Task.CompletedTask;

                OnlineUsers[username].Remove(connectionId);

                if(OnlineUsers[username].Count == 0){
                    OnlineUsers.Remove(username);
                }
            }

            return Task.CompletedTask;
        }

        public Task<string[]> GetOnlineUsers(){
            string[] onlineUsers;
            lock(OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            return Task.FromResult(onlineUsers);
        }

        public static Task<List<string>> GetConnectionsForUser (string username)
        {
            List<string> connectionIds;

            lock(OnlineUsers)
            {
                connectionIds = OnlineUsers.GetValueOrDefault(username);
            }
            return Task.FromResult(connectionIds);
        }
    }

}