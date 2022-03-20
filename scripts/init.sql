-- Markets
create table if not exists markets(
  id         integer                        not null primary key autoincrement,
  name       text                           not null,
  comment    text,
  stooq_id   integer,
  created_at text default CURRENT_TIMESTAMP not null
);

-- Companies
create table if not exists companies(
  id         integer                        not null primary key autoincrement,
  code       text                           not null,
  name       text                           not null,
  created_at text default CURRENT_TIMESTAMP not null,
  market_id  integer                        not null references markets(id)
);

-- Stocks
create table if not exists stocks(
  company_id integer not null references companies(id),
  date       text    not null,
  open       real    not null,
  high       real    not null,
  low        real    not null,
  close      real    not null,
  volume     real,
  primary key(company_id, date)
);

-- Groups
create table if not exists groups(
  id         integer primary key autoincrement not null,
  name       text                              not null,
  comment    text,
  created_at text default CURRENT_TIMESTAMP    not null,
  unique(name)
);

-- Group - Companies cross table
create table if not exists group_companies(
  group_id   integer not null references groups(id),
  company_id integer not null references companies(id),
  primary key(group_id, company_id)
);
