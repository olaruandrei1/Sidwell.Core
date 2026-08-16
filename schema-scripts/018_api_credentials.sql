CREATE TABLE api_credentials (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    provider        VARCHAR(30) NOT NULL UNIQUE,
    encrypted_key   TEXT NOT NULL,
    rotated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);
