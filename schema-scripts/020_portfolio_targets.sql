-- Per-user — alocarea tinta pe ticker (cate actiuni vrea userul sa detina), comparata
-- cu holdings.shares curent pentru a arata cat mai are de cumparat/vandut pana la tinta.
CREATE TABLE portfolio_targets (
    user_id         UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    ticker_id       UUID NOT NULL REFERENCES tickers(id) ON DELETE CASCADE,
    target_shares   NUMERIC(20, 8) NOT NULL CHECK (target_shares >= 0),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, ticker_id)
);
