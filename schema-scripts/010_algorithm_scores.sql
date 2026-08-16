-- Global derivat — rezultatul algoritmilor. FARA user_id (D3): calculul e determinist din
-- ticker + fundamentale + filosofie, nu depinde de cine intreaba. Filosofia e dimensiune
-- a datelor, nu atribut al userului.
CREATE TABLE algorithm_scores (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticker_id       UUID NOT NULL REFERENCES tickers(id) ON DELETE CASCADE,
    algorithm_id    UUID NOT NULL REFERENCES algorithms(id) ON DELETE CASCADE,
    -- 'ALL' pentru algoritmii cu algorithms.is_philosophy_dependent = false (marea majoritate);
    -- una din cele 4 filosofii doar pentru algoritmi philosophy-dependent (azi doar composite).
    philosophy      VARCHAR(30) NOT NULL CHECK (philosophy IN ('ALL', 'BALANCED', 'MOMENTUM', 'MEAN_REVERSION', 'FUNDAMENTAL')),
    as_of_date      DATE NOT NULL,
    -- NULL cand algoritmul nu e aplicabil (date insuficiente) — vezi details->>'applicable'.
    -- Niciodata un numar inventat in locul lui NULL.
    score           NUMERIC(10, 4),
    details         JSONB NOT NULL DEFAULT '{}'::jsonb,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_algorithm_scores_dims UNIQUE (ticker_id, algorithm_id, philosophy, as_of_date)
);

CREATE INDEX idx_algorithm_scores_ticker_id ON algorithm_scores (ticker_id);
