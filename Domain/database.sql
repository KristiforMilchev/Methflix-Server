-- we don't know how to generate root <with-no-name> (class Root) :(

comment on database postgres is 'default administrative connection database';

create table "UploadedFiles"
(
    "Id"        integer not null
        constraint "UploadedFiles_pk"
            primary key,
    "Name"      varchar(100),
    "CreatedBy" integer,
    "CreatedAt" timestamp
);

alter table "UploadedFiles"
    owner to postgres;

create table "FileExtensions"
(
    "Id"        serial
        constraint "FileExtensions_pk"
            primary key,
    name        varchar(10),
    "CreatedBy" integer
);

alter table "FileExtensions"
    owner to postgres;

create table "AccessRole"
(
    "Id"        serial
        constraint "AccessRole_pk"
            primary key,
    "Name"      varchar(50),
    "Priority"  integer,
    "CreatedAt" timestamp
);

alter table "AccessRole"
    owner to postgres;

create table "Accounts"
(
    "Id"        serial
        constraint "Accounts_pk"
            primary key,
    "Name"      varchar(50),
    "RoleId"    integer
        constraint "Accounts_AccessRole_Id_fk"
            references "AccessRole",
    "CreatedBy" integer,
    "CreatedAt" timestamp
);

alter table "Accounts"
    owner to postgres;

create table "Category"
(
    "Id"        serial
        constraint "Category_pk"
            primary key,
    "Name"      varchar(100),
    "CreatedBy" integer not null
        constraint "Category_Accounts_Id_fk"
            references "Accounts",
    "CreatedAt" timestamp,
    "UpdatedBy" integer
        constraint "Category_Accounts_Id_fk2"
            references "Accounts",
    "UpdatedAt" integer,
    "IsDeleted" integer
);

alter table "Category"
    owner to postgres;

create table "DTorrents"
(
    "Id"           integer not null
        constraint "DTorrents_pk"
            primary key,
    "Name"         varchar(100),
    "Location"     varchar(500),
    "CreatedAt"    timestamp,
    "CreatedBy"    integer
        constraint "DTorrents_Accounts_Id_fk"
            references "Accounts",
    "IsDownloaded" boolean,
    "IsSeeding"    boolean,
    "IsDeleted"    boolean
);

alter table "DTorrents"
    owner to postgres;

create table "TvShows"
(
    "Id"         serial
        constraint "TvShows_pk"
            primary key,
    "Name"       integer,
    "Seasons"    integer,
    "CreatedBy"  integer
        constraint "TvShows_Accounts_Id_fk"
            references "Accounts",
    "CategoryId" integer,
    "CreatedAt"  timestamp
);

alter table "TvShows"
    owner to postgres;

create table "Movies"
(
    "Id"         serial
        constraint "Movies_pk"
            primary key,
    "Name"       varchar(100),
    "TimeData"   interval,
    "Path"       varchar(500),
    "CategoryId" integer
        constraint "Movies_Category_Id_fk"
            references "Category",
    "TorrentId"  integer
        constraint "Movies_DTorrents_Id_fk"
            references "DTorrents",
    "DownloadId" integer
        constraint "Movies_UploadedFiles_Id_fk"
            references "UploadedFiles",
    "Extension"  integer
        constraint "Movies_FileExtensions_Id_fk"
            references "FileExtensions",
    "Thumbnail"  varchar(500),
    "TvShowId"   integer
        constraint "Movies_TvShows_Id_fk"
            references "TvShows"
);

alter table "Movies"
    owner to postgres;

create table "AssociatedSeasonEpisodes"
(
    "Id"       serial
        constraint "AssociatedSeasonEpisodes_pk"
            primary key,
    "TvShowId" integer
        constraint "AssociatedSeasonEpisodes_TvShows_Id_fk"
            references "TvShows",
    "MovieId"  integer
        constraint "AssociatedSeasonEpisodes_Movies_Id_fk"
            references "Movies",
    "Season"   integer
);

alter table "AssociatedSeasonEpisodes"
    owner to postgres;

create table "AccessKeys"
(
    "Id"        serial
        constraint "AccessKeys_pk"
            primary key,
    "Key"       varchar(300),
    "AccountId" integer not null
        constraint "AccessKeys_Accounts_Id_fk"
            references "Accounts",
    "CreatedBy" integer
);

alter table "AccessKeys"
    owner to postgres;

