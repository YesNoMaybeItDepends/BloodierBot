SELECT 
games.Id AS RunningGame_Id,
games.Half AS half,
games.Turn AS turn,
games.Division as division,
games.TournamentId as Tournament_Id,
tourneys.Name as t_name,
teams.Id as RunningGameTeam_Id,
teams.Side as side,
teams.Name as name,
teams.Coach as coach,
teams.Race as race,
teams.Tv as tv,
teams.Rating as rating,
teams.Score as score,
teams.Logo as logo,
teams.LogoLarge as logolarge
FROM RunningGames AS games
INNER JOIN RunningGameTeams AS teams ON (teams.RunningGameId = games.Id) 
LEFT OUTER JOIN Tournaments AS tourneys ON (tourneys.Id = games.TournamentId) 
ORDER BY games.Id