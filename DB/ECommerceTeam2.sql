CREATE DATABASE	ECommerceDB;

USE ECommerceDB;


CREATE TABLE Users(
	id_user UNIQUEIDENTIFIER PRIMARY KEY,
    email NVARCHAR(100) NOT NULL UNIQUE,
    user_role NVARCHAR(50)  NOT NULL,
	CONSTRAINT CK_ruolo CHECK (user_role in ('Seller','User') )
)

CREATE TABLE Sizes (
    id_size INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(50) NOT NULL UNIQUE,
);

CREATE TABLE Colors (
    id_color INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(50) NOT NULL UNIQUE
);


CREATE TABLE Materials (
    id_material INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Category (
    id_category INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Products (
	id_prod UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	brand NVARCHAR (50) NOT NULL,
	gender NVARCHAR(1) NOT NULL,
	nome NVARCHAR (255) NOT NULL,
	id_category INT NOT NULL,
	descr NVARCHAR (1000) NULL,
	price DECIMAL (10,2) NOT NULL,
	stock INT NOT NULL,
	CONSTRAINT CK_presso CHECK (price >0),
	CONSTRAINT CK_stock CHECK (stock >=0),
	CONSTRAINT CK_gender CHECK (gender IN ('F','M','U')),
	CONSTRAINT FK_Prod_Catogry FOREIGN KEY (id_category) REFERENCES Category (id_category),
)


CREATE TABLE ProdSize (
	id_prod UNIQUEIDENTIFIER NOT NULL,
	id_size INT NOT NULL,
	CONSTRAINT PK_ProdSize PRIMARY KEY (id_prod, id_size),
	CONSTRAINT FK_ProdSize_prod FOREIGN KEY (id_prod) REFERENCES Products (id_prod),
	CONSTRAINT FK_ProdSize_size FOREIGN KEY (id_size) REFERENCES Sizes (id_size),
)

CREATE TABLE ProdColor (
	id_prod UNIQUEIDENTIFIER NOT NULL,
	id_color INT NOT NULL,
	CONSTRAINT PK_ProdColor PRIMARY KEY (id_prod, id_color),
	CONSTRAINT FK_ProdColor_prod FOREIGN KEY (id_prod) REFERENCES Products (id_prod),
	CONSTRAINT FK_ProdColor_color FOREIGN KEY (id_color) REFERENCES Colors (id_color),
)

ALTER TABLE ProdColor ADD img_URL NVARCHAR(1000) NOT NULL

CREATE TABLE ProdMaterial (
	id_prod UNIQUEIDENTIFIER NOT NULL,
	id_material INT NOT NULL,
	percentage_mat DECIMAL (5,2) NOT NULL,
	CONSTRAINT PK_ProdMat PRIMARY KEY (id_prod, id_material),
	CONSTRAINT CK_percentage CHECK (percentage_mat BETWEEN 0 AND 100),
	CONSTRAINT FK_ProdMat_prod FOREIGN KEY (id_prod) REFERENCES Products (id_prod),
	CONSTRAINT FK_ProdMat_Mat FOREIGN KEY (id_material) REFERENCES Materials (id_material),
)


CREATE TABLE Orders (
	id_order INT IDENTITY (1,1) PRIMARY KEY,
	id_user UNIQUEIDENTIFIER NOT NULL,
	date_order DATETIME DEFAULT GETDATE(),
	total DECIMAL(10,2) not null,
	stateOrder NVARCHAR(50) NOT NULL,
	CONSTRAINT CK_totale CHECK (total >= 0),
	CONSTRAINT CK_stato CHECK (stateOrder IN ('in elaborazione','spedito','consegnato')),
	CONSTRAINT FK_Ordini_Utenti FOREIGN KEY (id_user) REFERENCES Users (id_user),
)

CREATE TABLE Cart (
	id_order INT IDENTITY (1,1) NOT NULL,
	id_prod UNIQUEIDENTIFIER NOT NULL,
	qt INT NOT NULL,
	price DECIMAL (10,2) NOT NULL,
	CONSTRAINT PK_ordineprodotto PRIMARY KEY (id_order, id_prod), 
	CONSTRAINT CK_quantita CHECK (qt >0),
	CONSTRAINT CK_prezzo_unitario CHECK (price >0),
	CONSTRAINT FK_OrdineProdotto_Ordine FOREIGN KEY (id_order) REFERENCES Orders (id_order),
	CONSTRAINT FK_OrdineProdotto_Prodotto FOREIGN KEY (id_prod) REFERENCES Products (id_prod) 
)

CREATE TABLE Ratings(
	id_rating UNIQUEIDENTIFIER PRIMARY KEY,
	id_prod UNIQUEIDENTIFIER NOT NULL,
	id_user UNIQUEIDENTIFIER NOT NULL,
	comment NVARCHAR(500), 
	rating INT NOT NULL,
	CreatedAt DATETIME DEFAULT GETDATE(),
	CONSTRAINT CK_rating CHECK (rating BETWEEN 1 AND 5),
	CONSTRAINT FK_Rating_Prodotto FOREIGN KEY (id_prod) REFERENCES Products (id_prod),
	CONSTRAINT FK_Rating_Users FOREIGN KEY (id_user) REFERENCES Users (id_user), 
)

INSERT INTO Sizes (nome) VALUES  
('XS'), 
('S'), 
('M'), 
('L'), 
('XL');

INSERT INTO Colors (nome) VALUES  
('Black'), 
('White'), 
('Pink'), 
('Red'), 
('Green'), 
('Yellow'), 
('Purple'), 
('Orange'), 
('Brown'), 
('Gold'), 
('Silver'), 
('Multicolor'), 
('Blue');


INSERT INTO Materials (nome) VALUES 
('Cotone'),
('Lino'),
('Lana'),
('Seta'),
('Poliestere'),
('Poliammide'),
('Acrilico'),
('Elastan');

INSERT INTO Category(nome) VALUES 
('Top'),
('Vestiti'),
('Jeans e Pantaloni'),
('Maglieria e felpe'),
('Gonne'),
('Soprabiti'),
('Pantaloncini'),
('Pezzi interi');

SELECT * FROM Products


-- Categoria 1: Top
INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) 
VALUES 
  ('Nike', 'M', 'T-shirt con logo', 1, 'T-shirt da uomo in cotone con logo Nike sul petto.', 29.99, 100),
  ('Uniqlo', 'M', 'Polo in cotone', 1, 'Polo casual in cotone, comoda per tutte le occasioni.', 39.99, 110),
  ('Puma', 'M', 'Maglietta basic', 1, 'Maglietta basic da uomo in cotone, comoda e versatile.', 19.99, 200),
  ('H&M', 'U', 'T-shirt unisex', 1, 'T-shirt unisex, disponibile in diverse varianti di colore.', 24.99, 200),
  ('Superdry', 'M', 'T-shirt vintage', 1, 'T-shirt uomo con grafica vintage sul davanti.', 29.99, 200);

-- Categoria 2: Vestiti
INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) 
VALUES 
  ('Zara', 'F', 'Abito estivo floreale', 2, 'Abito leggero e fresco con motivi floreali, perfetto per l''estate.', 69.99, 80),
  ('Mango', 'F', 'Vestito lungo', 2, 'Vestito lungo in tessuto fluido, perfetto per una serata elegante.', 89.99, 75),
  ('H&M', 'F', 'Abito a trapezio', 2, 'Abito a trapezio in tessuto morbido, adatto a ogni occasione.', 49.99, 150),
  ('Zara', 'F', 'Giacca di jeans', 2, 'Giacca di jeans da donna con dettagli vintage.', 59.99, 50),
  ('Mango', 'F', 'Gonna midi', 2, 'Gonna midi elegante in tessuto fluido, perfetta per ogni occasione.', 59.99, 120);

-- Categoria 3: Jeans e Pantaloni
INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) 
VALUES 
  ('Levi''s', 'M', 'Jeans slim fit', 3, 'Jeans uomo in denim stretch, con vestibilità slim fit.', 59.99, 90),
  ('Calvin Klein', 'F', 'Jeans skinny', 3, 'Jeans da donna skinny, in denim elasticizzato per una vestibilità perfetta.', 79.99, 130),
  ('Uniqlo', 'M', 'Pantaloni casual', 3, 'Pantaloni casual unisex, ideali per il tempo libero.', 59.99, 100),
  ('Puma', 'M', 'Jeans comfort', 3, 'Jeans uomo con tasche laterali, per un look casual e comodo.', 49.99, 150),
  ('Reebok', 'F', 'Pantaloni sportivi', 3, 'Pantaloni sportivi da donna in tessuto elasticizzato per la palestra.', 49.99, 150);


-- Categoria 4: Maglieria e Felpe
INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) 
VALUES 
  ('Nike', 'M', 'Felpa con cappuccio', 4, 'Felpa da uomo con cappuccio, perfetta per il tempo libero.', 59.99, 140),
  ('H&M', 'M', 'Felpa con zip', 4, 'Felpa con zip da uomo, comoda e versatile.', 39.99, 150),
  ('Uniqlo', 'M', 'Felpa basic', 4, 'Felpa da uomo in cotone, calda e comoda per ogni giorno.', 49.99, 100),
  ('Adidas', 'F', 'Felpa sportiva', 4, 'Felpa da donna in materiale tecnico, ideale per attività sportive.', 69.99, 80),
  ('Calvin Klein', 'M', 'Maglione in lana', 4, 'Maglione da uomo in lana merino, perfetto per l''inverno.', 119.99, 70);


-- Categoria 5: Gonne
INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) 
VALUES 
  ('Mango', 'F', 'Gonna a pieghe', 5, 'Gonna a pieghe lunga, elegante e sofisticata.', 49.99, 150),
  ('H&M', 'F', 'Gonna a tubino', 5, 'Gonna a tubino da donna, perfetta per un look elegante.', 39.99, 120),
  ('Bershka', 'F', 'Gonna corta', 5, 'Gonna corta da donna, ideale per l''estate.', 29.99, 180),
  ('Zara', 'F', 'Gonna midi', 5, 'Gonna midi elegante, perfetta per il lavoro o una serata fuori.', 59.99, 100),
  ('Mango', 'F', 'Gonna lunga', 5, 'Gonna lunga da donna, comoda e fresca per le giornate estive.', 69.99, 70);
 

-- Categoria 6: Soprabiti
INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) 
VALUES 
  ('Diesel', 'M', 'Cappotto in lana', 6, 'Cappotto da uomo in lana con tasche laterali.', 149.99, 40),
  ('Superdry', 'F', 'Giubbotto imbottito', 6, 'Giaccone imbottito da donna, ideale per l''inverno.', 139.99, 80),
  ('Levi''s', 'M', 'Giubbotto di pelle', 6, 'Giubbotto di pelle da uomo, robusto e alla moda.', 169.99, 50),
  ('Diesel', 'M', 'Trench coat', 6, 'Trench coat da uomo in cotone, perfetto per ogni stagione.', 179.99, 50),
  ('Zara', 'F', 'Abito lungo', 2, 'Abito lungo elegante, ideale per cerimonie e occasioni speciali.', 99.99, 70);

