CREATE TABLE IF NOT EXISTS stock_quotes (
    id          BIGSERIAL PRIMARY KEY,
    symbol      VARCHAR(10)    NOT NULL,
    price       DECIMAL(12, 4) NOT NULL,
    open        DECIMAL(12, 4),
    high        DECIMAL(12, 4),
    low         DECIMAL(12, 4),
    volume      BIGINT,
    timestamp   TIMESTAMPTZ    NOT NULL,
    ingested_at TIMESTAMPTZ    DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS moving_averages (
    id               BIGSERIAL PRIMARY KEY,
    symbol           VARCHAR(10)    NOT NULL,
    timestamp        TIMESTAMPTZ    NOT NULL,
    price            DECIMAL(12, 4) NOT NULL,
    ma_5             DECIMAL(12, 4),
    ma_20            DECIMAL(12, 4),
    ma_50            DECIMAL(12, 4),
    price_change_pct DECIMAL(8, 4),
    processed_at     TIMESTAMPTZ    DEFAULT NOW()
);

CREATE INDEX idx_stock_quotes_symbol_ts    ON stock_quotes(symbol, timestamp DESC);
CREATE INDEX idx_moving_averages_symbol_ts ON moving_averages(symbol, processed_at DESC);
