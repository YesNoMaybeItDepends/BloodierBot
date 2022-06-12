SELECT rg.Id
FROM RunningGames as rg
INNER JOIN RunningGameTeams as rgt ON rgt.RunningGameId = rg.Id
INNER JOIN ScheduledMatchTeams as smt ON smt.TeamId = rgt.Id
WHERE smt.TeamId = @Team1 

INTERSECT 

SELECT rg.Id
FROM RunningGames as rg
INNER JOIN RunningGameTeams as rgt ON rgt.RunningGameId = rg.Id
INNER JOIN ScheduledMatchTeams as smt ON smt.TeamId = rgt.Id
AND smt.TeamId = @Team2