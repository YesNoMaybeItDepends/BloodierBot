INSERT INTO Bets
(UserId, MatchId, AteamScore, BteamScore, Money)
VALUES
(@UserId, @MatchId, @AteamScore, @BteamScore, @Money)
ON CONFLICT (UserId, MatchId) DO UPDATE SET 
UserId = excluded.UserId, 
MatchId = excluded.MatchId, 
AteamScore = excluded.AteamScore,
BteamScore = excluded.BteamScore,
Money = excluded.Money;
