-- Global — cursuri valutare zilnice vs RON. Alimenteaza conversia holdings/tranzactii
-- multi-valuta (BVB in RON, international in USD/EUR/etc.) si fx_rate_at_execution
-- de pe transactions (decizia M2).
CREATE TABLE exchange_rates (
    currency        CHAR(3) NOT NULL,
    rate_date       DATE NOT NULL,
    rate_to_ron     NUMERIC(18, 6) NOT NULL,
    source          VARCHAR(30) NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (currency, rate_date)
);

CREATE INDEX idx_exchange_rates_currency_date_desc ON exchange_rates (currency, rate_date DESC);
