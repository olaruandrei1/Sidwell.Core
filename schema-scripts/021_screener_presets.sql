-- Per-user — seturi de criterii salvate pentru Screener (filtre pe scoruri/fundamentale),
-- ca userul sa nu reconfigureze filtrele de la zero la fiecare vizita.
CREATE TABLE screener_presets (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name            VARCHAR(100) NOT NULL,
    criteria        JSONB NOT NULL DEFAULT '{}'::jsonb,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_screener_presets_user_name UNIQUE (user_id, name)
);

CREATE INDEX idx_screener_presets_user_id ON screener_presets (user_id);
