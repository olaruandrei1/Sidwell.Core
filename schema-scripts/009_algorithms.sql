CREATE TABLE algorithms (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name                    VARCHAR(100) NOT NULL,
    version                 VARCHAR(20) NOT NULL,
    is_active               BOOLEAN NOT NULL DEFAULT true,
    -- Majoritatea algoritmilor fundamentali sunt deterministi din ticker + fundamentale,
    -- indiferent de filosofia de scoring (BALANCED/MOMENTUM/MEAN_REVERSION/FUNDAMENTAL).
    -- Doar algo_composite depinde de filosofie (amesteca scorul tehnic, care e philosophy-dependent).
    -- Randurile din algorithm_scores pentru algoritmi cu is_philosophy_dependent = false
    -- se scriu mereu cu philosophy = 'ALL' (impus in Database/Procedures, vezi fn_upsert_algorithm_score).
    is_philosophy_dependent BOOLEAN NOT NULL DEFAULT false,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_algorithms_name_version UNIQUE (name, version)
);
