CREATE TABLE fundamentals (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticker_id               UUID NOT NULL REFERENCES tickers(id) ON DELETE CASCADE,
    as_of_date              DATE NOT NULL,
    period                  VARCHAR(10) NOT NULL, -- 'FY' | 'Q1' | 'Q2' | 'Q3' | 'Q4'

    revenue                 NUMERIC(20, 2),
    net_income              NUMERIC(20, 2),
    gross_profit            NUMERIC(20, 2),
    ebit                    NUMERIC(20, 2),
    ebitda                  NUMERIC(20, 2),
    total_assets            NUMERIC(20, 2),
    total_liabilities       NUMERIC(20, 2),
    total_equity            NUMERIC(20, 2),
    retained_earnings       NUMERIC(20, 2), -- Altman Z (component B)
    current_assets          NUMERIC(20, 2),
    current_liabilities     NUMERIC(20, 2),
    long_term_debt          NUMERIC(20, 2),
    total_debt              NUMERIC(20, 2), -- long-term + short-term interest-bearing; falls back to long_term_debt (Greenblatt EV)
    cash                    NUMERIC(20, 2), -- cash & short-term investments (Greenblatt EV)
    operating_cash_flow     NUMERIC(20, 2),
    capex                   NUMERIC(20, 2), -- positive magnitude (DCF free cash flow)
    free_cash_flow          NUMERIC(20, 2),
    eps                     NUMERIC(18, 6),
    shares_outstanding      BIGINT,
    dividend_per_share      NUMERIC(18, 6),
    dividend_yield          NUMERIC(10, 6), -- fraction, e.g. 0.02 = 2% (DDM applicability)
    dividend_growth         NUMERIC(10, 6), -- fraction, 5Y avg (DDM Gordon growth)
    book_value_per_share    NUMERIC(18, 6),
    market_cap              NUMERIC(24, 2),
    pe_ratio                NUMERIC(12, 4),
    roe                     NUMERIC(12, 6),

    -- Exclusiv pentru Beneish M-Score (Sidwell.Core/algs-scripts/013_algo_beneish_m.sql).
    -- Toate vin din acelasi raspuns SEC EDGAR companyfacts deja folosit pentru restul
    -- coloanelor — zero cereri HTTP in plus.
    -- TODO (Sidwell.Sync): fetcher-ul SEC EDGAR trebuie sa populeze aceste 4 coloane. Taguri
    -- XBRL de mapat (acelasi tipar ca metricTags din sec_edgar.go v2):
    --   accounts_receivable -> AccountsReceivableNetCurrent
    --   ppe_net              -> PropertyPlantAndEquipmentNet
    --   depreciation         -> DepreciationDepletionAndAmortization
    --   sga_expense          -> SellingGeneralAndAdministrativeExpense
    accounts_receivable     NUMERIC(20, 2),
    ppe_net                 NUMERIC(20, 2),
    depreciation            NUMERIC(20, 2),
    sga_expense             NUMERIC(20, 2),

    raw                     JSONB NOT NULL DEFAULT '{}'::jsonb,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_fundamentals_ticker_date_period UNIQUE (ticker_id, as_of_date, period)
);

CREATE INDEX idx_fundamentals_ticker_id ON fundamentals (ticker_id);
