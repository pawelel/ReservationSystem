-- ============================================================
-- usp_CreateReservation
--
-- Concurrency safety:
--   The overlap-check SELECT uses UPDLOCK + HOLDLOCK hints so
--   that the check → insert sequence is atomic.
--
--   UPDLOCK  - converts shared locks to update locks, preventing
--              two concurrent transactions from both reading
--              "no conflict" for the same desk at the same time.
--   HOLDLOCK - escalates to SERIALIZABLE for this statement,
--              acquiring key-range locks that block phantom inserts
--              until the transaction commits.
--
-- Interval logic: reservation is [StartAt, EndAt).
--   Two intervals [a,b) and [c,d) overlap iff a < d AND c < b.
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.usp_CreateReservation
    @DeskId           INT,
    @UserId           INT,
    @StartAt          DATETIME2(0),
    @EndAt            DATETIME2(0),
    @NewReservationId INT           OUTPUT,
    @ErrorMessage     NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @NewReservationId = NULL;
    SET @ErrorMessage     = NULL;

    BEGIN TRANSACTION;
    BEGIN TRY

        IF NOT EXISTS (SELECT 1 FROM dbo.Desks WHERE Id = @DeskId)
        BEGIN
            SET @ErrorMessage = 'Desk not found.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @UserId)
        BEGIN
            SET @ErrorMessage = 'User not found.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        DECLARE @Overlapping INT;

        SELECT @Overlapping = COUNT(1)
        FROM dbo.Reservations WITH (UPDLOCK, HOLDLOCK)
        WHERE DeskId = @DeskId
          AND Status = 0            -- 0 = Active
          AND StartAt < @EndAt
          AND EndAt   > @StartAt;

        IF @Overlapping > 0
        BEGIN
            SET @ErrorMessage = 'The desk is already reserved for the requested time slot.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        INSERT INTO dbo.Reservations (DeskId, UserId, StartAt, EndAt, Status)
        VALUES (@DeskId, @UserId, @StartAt, @EndAt, 0);

        SET @NewReservationId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ErrorMessage = ERROR_MESSAGE();
        THROW;
    END CATCH
END;
