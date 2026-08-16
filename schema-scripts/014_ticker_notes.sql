CREATE TABLE ticker_notes (
    user_id         UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    ticker_id       UUID NOT NULL REFERENCES tickers(id) ON DELETE CASCADE,
    body            TEXT NOT NULL,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, ticker_id)
);
