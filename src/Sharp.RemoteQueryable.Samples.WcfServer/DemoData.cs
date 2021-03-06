﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Linq;
using Sharp.RemoteQueryable.Samples.Model;

namespace Sharp.RemoteQueryable.Samples.WcfServer
{
  public static class DemoData
  {
    private static readonly Random random = new Random(DateTime.UtcNow.Millisecond);

    private static int GroupMemberCount = 4;

    private static string[] DemoNames = new[]
    {
      "Lemuel Garnes",
      "Justa Rosenbalm",
      "Yolanda Gilbert",
      "Hal Coll",
      "Toya Paterson",
      "Sandie Ronco",
      "Laila Masker",
      "Gerald Torgeson",
      "Carole Harting",
      "Lilli Croke",
      "Hazel Wetherell",
      "Marina Hawker",
      "Ettie Portales",
      "Sheryl Nero",
      "Ruthie Campagna",
      "Frederic Leland",
      "Shanon Tunstall",
      "Sha Henery",
      "Caroll Shuford",
      "Jesica Kielbasa",
      "Cecelia Heist",
      "Daniele Commons",
      "Eura Tusa",
      "Estelle Long",
      "Nam Rampton",
      "Vanna Barger",
      "Jama Buser",
      "Vasiliki Biro",
      "Pasty Burbage",
      "Precious Endo",
      "Alease Hemminger",
      "David Howlett",
      "Denita Jordahl",
      "Kandace Brummitt",
      "Idella Fritch",
      "Dylan Bushong",
      "Mabel Golston",
      "Trevor Navarette",
      "Isabell Greenlaw",
      "Ophelia Lapan",
      "Jerrold Jarboe",
      "Agripina Westlake",
      "Yetta Opitz",
      "Mireille Stinnett",
      "Emerald Pedraza",
      "Lavern Glassman",
      "Chung Postel",
      "Juliette Rinaldo",
      "Bruno Covington",
      "Beau Star",
      "Tawana Hartsfield",
      "Lashay Folson",
      "Tina Braud",
      "Marjorie Carmean",
      "Maragret Sons",
      "Arlena Arellano",
      "Tiesha Bellamy",
      "Donte Ansley",
      "Saturnina Whitsitt",
      "Berenice Householder",
      "Dorla Martins",
      "Racheal Lasso",
      "Luella Maclean",
      "Devorah Panetta",
      "Maryellen Prejean",
      "Edith Sumrall",
      "Queenie Champion",
      "Iliana Plasencia",
      "Jewel Miner",
    };

    public static void Generate()
    {
      RemoveExistingDatabaseIfNeed();
      var developers = GenerateDevelopers();
      GenerateTeams(developers);
    }

    private static void RemoveExistingDatabaseIfNeed()
    {
      if (File.Exists("demo.db"))
        File.Delete("demo.db");
    }

    private static void GenerateTeams(IEnumerable<Developer> developers)
    {
      var developersList = developers.ToList();
      var leadersCount = developersList.Count(d => d is Teamleader);
      using (var session = NHibernateHelper.OpenSession())
      {
        for (int i = 0; i < developersList.Count(); i++)
        {
          var team = new Team { Title = $"Team {i}", Id = leadersCount-- };
          var currentDeveloper = developersList[i];
          while (i < developersList.Count)
          {
            team.Developers.Add(currentDeveloper);
            currentDeveloper = developersList[i];
            var leader = currentDeveloper as Teamleader;
            if (leader != null)
              team.Leader = leader;

            i++;
          }

          session.Save(team);
        }
        session.Flush();
      }
    }

    private static IEnumerable<Developer> GenerateDevelopers()
    {
      using (var session = NHibernateHelper.OpenSession())
      {
        for (int i = 0; i < DemoNames.Length; i++)
        {
          var currentId = i;
          var developer = ResolveDeveloper(currentId, session);

          var firstWorkItem = new WorkItem() { Priority = random.Next() };
          var secondWorkItem = new WorkItem() { Priority = random.Next() };

          session.Save(firstWorkItem);
          session.Save(secondWorkItem);
          session.Save(developer);
          session.Flush();
          yield return developer;
        }
      }
    }

    private static Developer ResolveDeveloper(int currentId, ISession session)
    {
      Developer developer;
      if (currentId % GroupMemberCount == 0)
      {
        developer = GetExistingOrCreate<Teamleader>(session, currentId);
        developer.SkillLevel = random.Next();
      }
      else
        developer = GetExistingOrCreate<Developer>(session, currentId);

      return developer;
    }

    private static Developer GetExistingOrCreate<T>(ISession session, int nameId) where T : Developer
    {
      var developer = session.Query<T>().FirstOrDefault(d => d.Name == DemoNames[nameId]);
      if (developer == null)
      {
        developer = Activator.CreateInstance<T>();
        developer.Name = DemoNames[nameId];
        developer.SkillLevel = random.Next();
      }
      return developer;
    }
  }
}
