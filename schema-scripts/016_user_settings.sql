CREATE TABLE user_settings (
    user_id     UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    key         VARCHAR(60) NOT NULL,
    value       TEXT NOT NULL,
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, key)
);
