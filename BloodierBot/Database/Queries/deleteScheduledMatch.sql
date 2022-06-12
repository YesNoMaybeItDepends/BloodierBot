DELETE FROM Teams
WHERE TeamId IN (
	SELECT TeamId
	FROM ScheduledMatchTeams
	WHERE ScheduledMatchId = @Id
);

DELETE FROM ScheduledMatchTeams
WHERE ScheduledMatchId = @Id;

DELETE FROM ScheduledGames
WHERE Id = @Id;