CREATE TABLE users (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email           VARCHAR(320) NOT NULL UNIQUE,
    display_name    VARCHAR(120),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);
