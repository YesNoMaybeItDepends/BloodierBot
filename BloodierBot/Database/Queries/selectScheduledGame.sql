SELECT * FROM ScheduledGames as sg
INNER JOIN ScheduledMatchTeams as smt on smt.ScheduledMatchId = sg.Id
INNER JOIN Teams as t on t.TeamId = smt.TeamId
WHERE sg.Id = @Id