
CREATE TABLE IF NOT EXISTS "role" (
	"id" INTEGER NOT NULL GENERATED ALWAYS AS IDENTITY,
	"name" TEXT NULL DEFAULT NULL,
	"normalized_name" TEXT NULL DEFAULT NULL,
	"concurrency_stamp" TEXT NULL DEFAULT NULL,
	PRIMARY KEY ("id")
);

CREATE TABLE IF NOT EXISTS "role_claim" (
	"id" INTEGER NOT NULL GENERATED ALWAYS AS IDENTITY,
	"role_id" INTEGER NOT NULL,
	"claim_type" TEXT NULL DEFAULT NULL,
	"claim_value" TEXT NULL DEFAULT NULL,
	PRIMARY KEY ("id"),
	CONSTRAINT "FK_role_claim_role_role_id" FOREIGN KEY ("role_id") REFERENCES "public"."role" ("id") ON UPDATE NO ACTION ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "user" (
	"id" BIGINT NOT NULL GENERATED ALWAYS AS IDENTITY,
	"user_name" TEXT NULL DEFAULT NULL,
	"normalized_user_name" TEXT NULL DEFAULT NULL,
	"email" TEXT NULL DEFAULT NULL,
	"normalized_email" TEXT NULL DEFAULT NULL,
	"email_confirmed" BOOLEAN NOT NULL,
	"password_hash" TEXT NULL DEFAULT NULL,
	"security_stamp" TEXT NULL DEFAULT NULL,
	"concurrency_stamp" TEXT NULL DEFAULT NULL,
	"phone_number" TEXT NULL DEFAULT NULL,
	"phone_number_confirmed" BOOLEAN NOT NULL,
	"two_factor_enabled" BOOLEAN NOT NULL,
	"lockout_end" TIMESTAMP NULL DEFAULT NULL,
	"lockout_enabled" BOOLEAN NOT NULL,
	"access_failed_count" INTEGER NOT NULL,
	PRIMARY KEY ("id")
);

CREATE TABLE IF NOT EXISTS "user_claim" (
	"id" INTEGER NOT NULL GENERATED ALWAYS AS IDENTITY,
	"user_id" BIGINT NOT NULL,
	"claim_type" TEXT NULL DEFAULT NULL,
	"claim_value" TEXT NULL DEFAULT NULL,
	PRIMARY KEY ("id"),
	CONSTRAINT "FK_user_claim_user_user_id" FOREIGN KEY ("user_id") REFERENCES "public"."user" ("id") ON UPDATE NO ACTION ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "user_login" (
	"login_provider" TEXT NOT NULL,
	"provider_key" TEXT NOT NULL,
	"provider_display_name" TEXT NULL DEFAULT NULL,
	"user_id" BIGINT NOT NULL,
	PRIMARY KEY ("login_provider", "provider_key"),
	CONSTRAINT "FK_user_login_user_user_id" FOREIGN KEY ("user_id") REFERENCES "public"."user" ("id") ON UPDATE NO ACTION ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "user_role" (
	"user_id" BIGINT NOT NULL,
	"role_id" INTEGER NOT NULL,
	PRIMARY KEY ("user_id", "role_id"),
	CONSTRAINT "FK_user_role_role_role_id" FOREIGN KEY ("role_id") REFERENCES "public"."role" ("id") ON UPDATE NO ACTION ON DELETE CASCADE,
	CONSTRAINT "FK_user_role_user_user_id" FOREIGN KEY ("user_id") REFERENCES "public"."user" ("id") ON UPDATE NO ACTION ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "user_token" (
	"user_id" BIGINT NOT NULL,
	"login_provider" TEXT NOT NULL,
	"name" TEXT NOT NULL,
	"value" TEXT NULL DEFAULT NULL,
	PRIMARY KEY ("user_id", "login_provider", "name"),
	CONSTRAINT "FK_user_token_user_user_id" FOREIGN KEY ("user_id") REFERENCES "public"."user" ("id") ON UPDATE NO ACTION ON DELETE CASCADE
);

CREATE UNIQUE INDEX "RoleNameIndex" ON "role" ("normalized_name");
CREATE INDEX "IX_role_claim_role_id" ON "role_claim" ("role_id");
CREATE INDEX "EmailIndex" ON "user" ("normalized_email");
CREATE UNIQUE INDEX "UserNameIndex" ON "user" ("normalized_user_name");
CREATE INDEX "IX_user_claim_user_id" ON "user_claim" ("user_id");
CREATE INDEX "IX_user_login_user_id" ON "user_login" ("user_id");
CREATE INDEX "IX_user_role_role_id" ON "user_role" ("role_id");

