﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class EventTeamData : MonoBehaviour {

    private Dictionary<string, EventData> eventData = new Dictionary<string, EventData>();

    private Dictionary<string, Dictionary<string, TeamInfo>> teamAtEventData = new Dictionary<string, Dictionary<string, TeamInfo>>();

    private Dictionary<EventData, Dictionary<TeamInfo, int[]>> matchData = new Dictionary<EventData, Dictionary<TeamInfo, int[]>>();

    public EventDropdown eventDropdown;
    public TeamDropdown teamDropdown;
    public MatchDropdown matchDropdown;

    public void Start()
    {
        if (File.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + "data.json"))
        {
            loadData(JsonUtility.FromJson<SyncData>(File.ReadAllText(Application.persistentDataPath + Path.DirectorySeparatorChar + "data.json")));
        }
    }

    public void clearData()
    {
        eventData.Clear();
        teamAtEventData.Clear();
        if (eventDropdown != null) { eventDropdown.clear(); }
        if (teamDropdown != null) { teamDropdown.clear(); }
    }

    public void loadData(SyncData dataToLoad)
    {
        foreach (EventData Event in dataToLoad.Events)
        {
            if (eventData.ContainsKey(Event.key)) continue;

            eventData.Add(Event.key, Event);
        }

        foreach (EventTeamList etl in dataToLoad.TeamsByEvent)
        {
            if (teamAtEventData.ContainsKey(etl.EventKey)) continue;

            teamAtEventData.Add(etl.EventKey, new Dictionary<string, TeamInfo>());
            foreach (TeamInfo ti in etl.TeamList)
            {
                if (teamAtEventData[etl.EventKey].ContainsKey(ti.key)) continue;

                teamAtEventData[etl.EventKey].Add(ti.key, ti);
            }
        }

        foreach (MatchSync ms in dataToLoad.EventMatches)
        {
            if (!matchData.ContainsKey(getEvent(ms.EventKey))) { matchData[getEvent(ms.EventKey)] = new Dictionary<TeamInfo, int[]>(); }

            List<int> matchNumbers = new List<int>();
            foreach (string mi in ms.Matches)
            {
                string[] parts = mi.Split('_');
                if (parts.Length != 2) continue;
                if (parts[1].IndexOf("qm") == -1) continue;
                matchNumbers.Add(int.Parse(parts[1].Substring(2)));
            }
            matchNumbers.Sort();
            matchData[getEvent(ms.EventKey)][getTeam("frc" + ms.TeamNumber, ms.EventKey)] = matchNumbers.ToArray();
        }


        if (eventDropdown != null) eventDropdown.refresh();
        if (teamDropdown != null) teamDropdown.refresh();
        if (matchDropdown != null) matchDropdown.refresh();
    }

    public EventData[] getEvents()
    {
        List<EventData> data = new List<EventData>();
        foreach (KeyValuePair<string,EventData> kvp in eventData)
        {
            data.Add(kvp.Value);
        }
        return data.ToArray();
    }

    public EventData getEvent(string eventKey)
    {
        if (!eventData.ContainsKey(eventKey)) return null;

        return eventData[eventKey];
    }

    public TeamInfo getTeam(string teamKey, EventData eventData) {
        return getTeam(teamKey, eventData.key);
    }

    public TeamInfo getTeam(int teamNumber, string eventKey)
    {
        return getTeam("frc" + teamNumber, eventKey);
    }

    public TeamInfo getTeam(int teamNumber, EventData eventData)
    {
        return getTeam("frc" + teamNumber, eventData.key);
    }

    public TeamInfo getTeam(string teamKey, string eventKey)
    {
        if (!teamAtEventData.ContainsKey(eventKey)) return null;
        if (!teamAtEventData[eventKey].ContainsKey(teamKey)) return null;

        return teamAtEventData[eventKey][teamKey];
    }

    public int[] getMatches(TeamInfo team, EventData eventData)
    {
        if (!matchData.ContainsKey(eventData)) return new int[] { -1 };
        if (!matchData[eventData].ContainsKey(team)) return new int[] { -2 };
        return matchData[eventData][team];
    }

    public EventData getSelectedEvent()
    {
        if (eventDropdown != null)
        {
            string eventKey = eventDropdown.GetComponent<Dropdown>().captionText.text.Split(' ')[0];
            if (eventData.ContainsKey(eventKey))
            {
                return eventData[eventKey];
            }
        }
        return null;
    }

    public TeamInfo getSelectedTeam()
    {
        if (teamDropdown != null)
        {
            string teamKey = "frc" + teamDropdown.GetComponent<Dropdown>().captionText.text.Split(' ')[0];
            if (teamAtEventData[getSelectedEvent().key].ContainsKey(teamKey))
            {
                return teamAtEventData[getSelectedEvent().key][teamKey];
            }
        }
        return null;
    }

    public TeamInfo[] getTeams(string eventKey)
    {
        if (teamAtEventData.ContainsKey(eventKey))
        {
            List<TeamInfo> tmpList = new List<TeamInfo>();
            foreach (KeyValuePair<string,TeamInfo> kvp in teamAtEventData[eventKey])
            {
                tmpList.Add(kvp.Value);
            }
            return tmpList.ToArray();
        }
        return null;
    }
}

[System.Serializable]
public class EventData
{
    //public string city;
    //public string country;
    //public DistrictInfo district;
    //public string end_date;
    public string event_code;
    public int event_type;
    public string key;
    public string name;
    //public string start_date;
    public int year;

    //public override string ToString()
    //{
    //    return "City: " + city + " Country: " + country + " District Abbr:" + district.abbreviation + " District Name: " + district.display_name + " District Key: " + district.key + " District Year: " + district.year + " End Date: " + end_date + " Event Code: " + event_code + " Event Type: " + event_type + " Key: " + key + " Name: " + name + " Start Date: " + start_date + " Year: " + year;
    //}
}

[System.Serializable]
public class EventTeamList
{
    public string EventKey;
    public TeamInfo[] TeamList;
}

[System.Serializable]
public class TeamInfo
{
    //public string city;
    //public string country;
    public string key;
    //public string name;
    public string nickname;
    //public string state_prov;
    public int team_number;

    //public override string ToString()
    //{
    //    return "City: " + city + " Country: " + country + " Key: " + key + " Name: " + name + " Nickname: " + nickname + " State/Prov: " + state_prov + " Team Number: " + team_number;
    //}
}

[System.Serializable]
public class DistrictInfo
{
    public string abbreviation;
    public string display_name;
    public string key;
    public int year;
}

[System.Serializable]
public class MatchSync
{
    public string EventKey;
    public int TeamNumber;
    public string[] Matches;
}

[System.Serializable]
public class AllianceInfo
{
    //public string[] dq_team_keys;
    //public int score;
    //public string[] surrogate_team_keys;
    //public string[] team_keys;
}