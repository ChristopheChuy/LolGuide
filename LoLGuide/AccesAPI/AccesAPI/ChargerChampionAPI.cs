﻿using Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Core
{
    public class ChargerChampionAPI : ChargerChampion
    {
        public  const String URLAPI = "https://global.api.pvp.net/api/lol/static-data/euw/v1.2/champion?locale=fr_FR&champData=info,lore,spells,tags&api_key=6c2ec2f3-72bb-4b83-a3d0-982da8adb4c0";

        /// <summary>
        /// Charge l'API LoL et récupère les données Json pour créer une liste de IChampion
        /// </summary>
        /// <returns> Une Task liste de IChampon </returns>
        public async Task<List<IChampion>> LoadChampion()
        {
            List<IChampion> listeChampions1 = new List<IChampion>();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.GetAsync(URLAPI);
                if (response.IsSuccessStatusCode)
                {
                    // Transformation de l'interprétation de la ressource
                    var champions = await response.Content.ReadAsStringAsync();
                    JObject o = JObject.Parse(champions);

                    var ListeChampions = o["data"].Children().Select(champ => new
                    {
                        champion = new Champion(
                            (int)champ.First()["id"],
                            (string)champ.First()["name"],
                            (string)champ.First()["title"],
                            ((string)champ.First()["lore"]).Replace("<br>", ""),
                            (string)champ.First()["key"],
                            new Dictionary<BarreDeForce, Int32>() {
                                { BarreDeForce.MAGIC,(int)champ.First()["info"]["magic"] },
                                { BarreDeForce.PHYS,(int)champ.First()["info"]["attack"] },
                                { BarreDeForce.DIFFICULTE,(int)champ.First()["info"]["difficulty"] },
                                { BarreDeForce.POINTSDEVIE,(int)champ.First()["info"]["defense"] },
                            },
                            champ.First["tags"].Select(tag => (string)tag).ToList()
                        ),
                        listeSorts = champ.First()["spells"].Children().Select(spell => new Sort(
                                   (string)spell["name"],
                                  (string)spell["description"],
                                   (string)spell["image"]["full"]
                                  )
                        ).ToList(),
                    }
                      ).OrderBy(champ => champ.champion.Nom).ToList();

                    foreach(var cs in ListeChampions){
                        cs.champion.addSort(cs.listeSorts);
                        Debug.WriteLine(cs.champion);
                    }
                    
                    // Transforme la liste de Champion en IChampion
                    listeChampions1 =  ListeChampions.Select(championSort => (IChampion)championSort.champion).ToList();
                }
            }
            Debug.WriteLine(listeChampions1);
            return listeChampions1;

        }
    }
}
