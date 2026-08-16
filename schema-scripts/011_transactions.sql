CREATE TABLE transactions (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    ticker_id       UUID NOT NULL REFERENCES tickers(id) ON DELETE RESTRICT,
    side            VARCHAR(4) NOT NULL CHECK (side IN ('BUY', 'SELL')),
    shares          NUMERIC(20, 8) NOT NULL CHECK (shares > 0),
    price           NUMERIC(18, 6) NOT NULL CHECK (price >= 0),
    fee             NUMERIC(18, 6) NOT NULL DEFAULT 0,
    -- Cursul RON la momentul executiei (decizia M2) — cost basis corect pentru tickere
    -- multi-valuta fara sa recalculezi retroactiv din exchange_rates cand cursul de azi se schimba.
    fx_rate_at_execution NUMERIC(18, 6),
    executed_at     TIMESTAMPTZ NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_transactions_user_executed ON transactions (user_id, executed_at DESC);
CREATE INDEX idx_transactions_ticker_id ON transactions (ticker_id);
