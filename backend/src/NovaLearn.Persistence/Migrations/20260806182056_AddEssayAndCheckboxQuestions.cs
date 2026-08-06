using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaLearn.Persistence.Migrations
{
    /// <summary>
    /// Adds essay and checkbox questions, and the manual marking they need.
    ///
    /// Hand-edited in two places, both of which the scaffolder got wrong or could not know:
    ///
    /// 1. It saw the old nullable uuid <c>SelectedOptionId</c> disappear and a nullable uuid
    ///    <c>MarkedById</c> appear, and guessed a rename. They are unrelated: one is the option a
    ///    learner picked, the other is who marked their essay. Left alone it would have relabelled
    ///    every recorded answer as a marker id. The column is now copied into the new
    ///    <c>SelectedOptionIds</c> list and then dropped.
    ///
    /// 2. <c>AttemptStatus.Submitted</c> was renamed to <c>Graded</c>. Enum values are stored as
    ///    strings, so existing rows would fail to parse. They are rewritten here.
    /// </summary>
    public partial class AddEssayAndCheckboxQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "QuizQuestions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MarkingGuidance",
                table: "QuizQuestions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MarkedAtUtc",
                table: "QuizAttempts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TextAnswer",
                table: "QuizAttemptAnswers",
                type: "character varying(20000)",
                maxLength: 20000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "QuizAttemptAnswers",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManuallyMarked",
                table: "QuizAttemptAnswers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MarkedAtUtc",
                table: "QuizAttemptAnswers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresManualMarking",
                table: "QuizAttemptAnswers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MarkedById",
                table: "QuizAttemptAnswers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedOptionIds",
                table: "QuizAttemptAnswers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            // Carry every recorded choice into the new list column before the old one goes.
            migrationBuilder.Sql(
                """
                UPDATE "QuizAttemptAnswers"
                SET "SelectedOptionIds" = "SelectedOptionId"::text
                WHERE "SelectedOptionId" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "SelectedOptionId",
                table: "QuizAttemptAnswers");

            // Attempts marked before essays existed are, by definition, fully marked.
            migrationBuilder.Sql(
                """
                UPDATE "QuizAttempts" SET "Status" = 'Graded' WHERE "Status" = 'Submitted';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizId_Status",
                table: "QuizAttempts",
                columns: new[] { "QuizId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswers_RequiresManualMarking_IsManuallyMarked",
                table: "QuizAttemptAnswers",
                columns: new[] { "RequiresManualMarking", "IsManuallyMarked" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_QuizId_Status",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttemptAnswers_RequiresManualMarking_IsManuallyMarked",
                table: "QuizAttemptAnswers");

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedOptionId",
                table: "QuizAttemptAnswers",
                type: "uuid",
                nullable: true);

            // Only a single selection can survive going back; a checkbox answer had no old home.
            migrationBuilder.Sql(
                """
                UPDATE "QuizAttemptAnswers"
                SET "SelectedOptionId" = "SelectedOptionIds"::uuid
                WHERE "SelectedOptionIds" IS NOT NULL AND "SelectedOptionIds" NOT LIKE '%,%';
                """);

            migrationBuilder.Sql(
                """
                UPDATE "QuizAttempts"
                SET "Status" = 'Submitted'
                WHERE "Status" IN ('Graded', 'PendingReview');
                """);

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "MarkingGuidance",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "MarkedAtUtc",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "QuizAttemptAnswers");

            migrationBuilder.DropColumn(
                name: "IsManuallyMarked",
                table: "QuizAttemptAnswers");

            migrationBuilder.DropColumn(
                name: "MarkedAtUtc",
                table: "QuizAttemptAnswers");

            migrationBuilder.DropColumn(
                name: "MarkedById",
                table: "QuizAttemptAnswers");

            migrationBuilder.DropColumn(
                name: "RequiresManualMarking",
                table: "QuizAttemptAnswers");

            migrationBuilder.DropColumn(
                name: "SelectedOptionIds",
                table: "QuizAttemptAnswers");

            migrationBuilder.AlterColumn<string>(
                name: "TextAnswer",
                table: "QuizAttemptAnswers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20000)",
                oldMaxLength: 20000,
                oldNullable: true);
        }
    }
}
