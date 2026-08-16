CREATE TABLE watchlist (
    user_id         UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    ticker_id       UUID NOT NULL REFERENCES tickers(id) ON DELETE CASCADE,
    added_at        TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, ticker_id)
);
