CREATE TABLE sync_jobs (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    source          VARCHAR(30) NOT NULL,
    status          VARCHAR(20) NOT NULL CHECK (status IN ('PENDING', 'RUNNING', 'SUCCEEDED', 'FAILED')),
    started_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    finished_at     TIMESTAMPTZ,
    error           TEXT
);

CREATE INDEX idx_sync_jobs_source_started ON sync_jobs (source, started_at DESC);
