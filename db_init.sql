-- =============================================================================
-- VIAMATICA PRUEBA TÉCNICA — Schema PostgreSQL 17
-- =============================================================================

-- -----------------------------------------------------------------------------
-- MÓDULO: ROLES Y USUARIOS
-- -----------------------------------------------------------------------------

CREATE TABLE rol (
    rolid       SERIAL          PRIMARY KEY,
    rolname     VARCHAR(50)     NOT NULL
);

CREATE TABLE userstatus (
    statusid    VARCHAR(3)      PRIMARY KEY,
    description VARCHAR(50)     NOT NULL
);

CREATE TABLE "user" (
    userid                  SERIAL          PRIMARY KEY,
    username                VARCHAR(20)     NOT NULL UNIQUE,
    email                   TEXT            NOT NULL,
    password                TEXT            NOT NULL,
    rol_rolid               INTEGER         NOT NULL    REFERENCES rol(rolid),
    creationdate            TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),
    usercreate              INTEGER                     REFERENCES "user"(userid),
    userapproval            INTEGER                     REFERENCES "user"(userid),
    dateapproval            TIMESTAMPTZ,
    userstatus_statusid     VARCHAR(3)      NOT NULL    REFERENCES userstatus(statusid),
    active                  BOOLEAN         NOT NULL    DEFAULT TRUE,
    deleted_at              TIMESTAMPTZ
);

-- -----------------------------------------------------------------------------
-- MÓDULO: CAJAS
-- -----------------------------------------------------------------------------

CREATE TABLE cash (
    cashid          SERIAL          PRIMARY KEY,
    cashdescription VARCHAR(50)     NOT NULL,
    active          BOOLEAN         NOT NULL    DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    deleted_at      TIMESTAMPTZ
);

CREATE TABLE usercash (
    user_userid     INTEGER         NOT NULL    REFERENCES "user"(userid),
    cash_cashid     INTEGER         NOT NULL    REFERENCES cash(cashid),
    is_active       BOOLEAN         NOT NULL    DEFAULT FALSE,
    assigned_at     TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),
    PRIMARY KEY (user_userid, cash_cashid)
);

-- -----------------------------------------------------------------------------
-- MÓDULO: CLIENTES
-- -----------------------------------------------------------------------------

CREATE TABLE client (
    clientid            SERIAL          PRIMARY KEY,
    name                VARCHAR(50)     NOT NULL,
    lastname            VARCHAR(50)     NOT NULL,
    identification      VARCHAR(13)     NOT NULL UNIQUE,
    email               VARCHAR(120)    NOT NULL,
    phonenumber         VARCHAR(13)     NOT NULL,
    address             VARCHAR(100)    NOT NULL,
    referenceaddress    VARCHAR(100)    NOT NULL,
    active              BOOLEAN         NOT NULL    DEFAULT TRUE,
    created_at          TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    deleted_at          TIMESTAMPTZ
);

-- -----------------------------------------------------------------------------
-- MÓDULO: SERVICIOS Y EQUIPOS
-- -----------------------------------------------------------------------------

CREATE TABLE service (
    serviceid           SERIAL          PRIMARY KEY,
    servicename         VARCHAR(100)    NOT NULL,
    servicedescription  VARCHAR(150)    NOT NULL,
    speed_mbps          INTEGER         NOT NULL,
    price               DECIMAL(10,2)   NOT NULL,
    active              BOOLEAN         NOT NULL    DEFAULT TRUE,
    created_at          TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    deleted_at          TIMESTAMPTZ
);

CREATE TABLE device (
    deviceid            SERIAL          PRIMARY KEY,
    devicename          VARCHAR(50)     NOT NULL,
    service_serviceid   INTEGER         NOT NULL    REFERENCES service(serviceid),
    active              BOOLEAN         NOT NULL    DEFAULT TRUE,
    created_at          TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    deleted_at          TIMESTAMPTZ
);

-- -----------------------------------------------------------------------------
-- MÓDULO: TURNOS Y ATENCIÓN
-- -----------------------------------------------------------------------------

CREATE TABLE attentiontype (
    attentiontypeid VARCHAR(3)      PRIMARY KEY,
    description     VARCHAR(100)    NOT NULL,
    prefix          VARCHAR(2)      NOT NULL UNIQUE
);

CREATE TABLE attentionstatus (
    statusid    SERIAL          PRIMARY KEY,
    description VARCHAR(30)     NOT NULL
);

CREATE TABLE turn (
    turnid          SERIAL          PRIMARY KEY,
    description     VARCHAR(6)      NOT NULL,
    date            TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),
    cash_cashid     INTEGER         NOT NULL    REFERENCES cash(cashid),
    usergestorid    INTEGER         NOT NULL    REFERENCES "user"(userid),
    created_at      TIMESTAMPTZ     NOT NULL    DEFAULT NOW()
);

CREATE TABLE attention (
    attentionid                     SERIAL          PRIMARY KEY,
    turn_turnid                     INTEGER         NOT NULL    REFERENCES turn(turnid),
    client_clientid                 INTEGER                     REFERENCES client(clientid),
    attentiontype_attentiontypeid   VARCHAR(3)      NOT NULL    REFERENCES attentiontype(attentiontypeid),
    attentionstatus_statusid        INTEGER         NOT NULL    REFERENCES attentionstatus(statusid),
    created_at                      TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),
    updated_at                      TIMESTAMPTZ
);

-- -----------------------------------------------------------------------------
-- MÓDULO: CONTRATOS Y PAGOS
-- -----------------------------------------------------------------------------

CREATE TABLE statuscontract (
    statusid    VARCHAR(3)      PRIMARY KEY,
    description VARCHAR(50)     NOT NULL
);

CREATE TABLE methodpayment (
    methodpaymentid SERIAL          PRIMARY KEY,
    description     VARCHAR(50)     NOT NULL
);

CREATE TABLE contract (
    contractid                      SERIAL          PRIMARY KEY,
    startdate                       TIMESTAMPTZ     NOT NULL,
    enddate                         TIMESTAMPTZ,
    service_serviceid               INTEGER         NOT NULL    REFERENCES service(serviceid),
    statuscontract_statusid         VARCHAR(3)      NOT NULL    REFERENCES statuscontract(statusid),
    client_clientid                 INTEGER         NOT NULL    REFERENCES client(clientid),
    methodpayment_methodpaymentid   INTEGER         NOT NULL    REFERENCES methodpayment(methodpaymentid),
    attention_attentionid           INTEGER                     REFERENCES attention(attentionid),
    parent_contractid               INTEGER                     REFERENCES contract(contractid),
    created_at                      TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),
    updated_at                      TIMESTAMPTZ,
    deleted_at                      TIMESTAMPTZ
);

CREATE TABLE payments (
    paymentid                       SERIAL          PRIMARY KEY,
    paydate                         TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),
    amount                          DECIMAL(10,2)   NOT NULL,
    client_clientid                 INTEGER         NOT NULL    REFERENCES client(clientid),
    contract_contractid             INTEGER         NOT NULL    REFERENCES contract(contractid),
    methodpayment_methodpaymentid   INTEGER         NOT NULL    REFERENCES methodpayment(methodpaymentid),
    attention_attentionid           INTEGER                     REFERENCES attention(attentionid),
    active                          BOOLEAN         NOT NULL    DEFAULT TRUE,
    created_at                      TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),
    updated_at                      TIMESTAMPTZ
);

-- =============================================================================
-- STORED PROCEDURE: Generación de descripción de turno
-- =============================================================================
-- Genera el código del turno: prefijo (2 letras) + secuencia diaria por caja
-- Ejemplo: si prefix = 'AC' y ya hay 3 turnos AC hoy en caja 1 → devuelve 'AC0004'
-- =============================================================================

CREATE OR REPLACE FUNCTION sp_generate_turn_description(
    p_cash_cashid           INTEGER,
    p_attentiontype_id      VARCHAR(3)
)
RETURNS VARCHAR(6)
LANGUAGE plpgsql
AS $$
DECLARE
    v_prefix        VARCHAR(2);
    v_sequence      INTEGER;
    v_description   VARCHAR(6);
BEGIN
    -- Obtener el prefijo del tipo de atención
    SELECT prefix INTO v_prefix
    FROM attentiontype
    WHERE attentiontypeid = p_attentiontype_id;

    IF v_prefix IS NULL THEN
        RAISE EXCEPTION 'Tipo de atención no encontrado: %', p_attentiontype_id;
    END IF;

    -- Contar turnos del mismo tipo en la misma caja en el día actual
    SELECT COUNT(*) + 1 INTO v_sequence
    FROM turn t
    INNER JOIN attention a ON a.turn_turnid = t.turnid
    WHERE t.cash_cashid = p_cash_cashid
      AND a.attentiontype_attentiontypeid = p_attentiontype_id
      AND DATE(t.date) = CURRENT_DATE;

    -- Validar que no supere 9999 turnos por día
    IF v_sequence > 9999 THEN
        RAISE EXCEPTION 'Límite de turnos diarios alcanzado para el tipo %', p_attentiontype_id;
    END IF;

    -- Construir descripción: prefijo + secuencia con ceros a la izquierda (4 dígitos)
    v_description := v_prefix || LPAD(v_sequence::TEXT, 4, '0');

    RETURN v_description;
END;
$$;

-- =============================================================================
-- DATOS SEMILLA
-- =============================================================================

-- Roles
INSERT INTO rol (rolname) VALUES
    ('Administrador'),
    ('Gestor'),
    ('Cajero'),
    ('Cliente');

-- Estados de usuario
INSERT INTO userstatus (statusid, description) VALUES
    ('ACT', 'Activo'),
    ('INA', 'Inactivo'),
    ('BLO', 'Bloqueado'),
    ('PEN', 'Pendiente de aprobación');

-- Tipos de atención
INSERT INTO attentiontype (attentiontypeid, description, prefix) VALUES
    ('AC',  'Atención al cliente',      'AC'),
    ('PS',  'Pago de servicio',         'PS'),
    ('CS',  'Cambio de servicio',       'CS'),
    ('CP',  'Cambio de forma de pago',  'CP'),
    ('CC',  'Cancelación de contrato',  'CC');

-- Estados de atención
INSERT INTO attentionstatus (description) VALUES
    ('Pendiente'),
    ('Atendido'),
    ('Cancelado');

-- Estados de contrato
INSERT INTO statuscontract (statusid, description) VALUES
    ('VIG', 'Vigente'),
    ('SUS', 'Sustituido'),
    ('REN', 'Renovado'),
    ('CAN', 'Cancelado');

-- Métodos de pago
INSERT INTO methodpayment (description) VALUES
    ('Efectivo'),
    ('Tarjeta de crédito'),
    ('Transferencia bancaria'),
    ('Débito automático');

-- Usuarios semilla (passwords encriptados por la app al arrancar)
INSERT INTO "user" (username, email, password, rol_rolid, userstatus_statusid) VALUES
    ('admin001',  'admin@viamatica.com',  'PENDING_ENCRYPTION', 1, 'ACT'),
    ('gestor001', 'gestor@viamatica.com', 'PENDING_ENCRYPTION', 2, 'ACT'),
    ('cajero001', 'cajero@viamatica.com', 'PENDING_ENCRYPTION', 3, 'ACT');

-- Servicios de internet
INSERT INTO service (servicename, servicedescription, speed_mbps, price) VALUES
    ('Plan Básico',      'Internet residencial 10 Mbps',    10,  19.99),
    ('Plan Estándar',    'Internet residencial 30 Mbps',    30,  29.99),
    ('Plan Avanzado',    'Internet residencial 50 Mbps',    50,  39.99),
    ('Plan Premium',     'Internet residencial 100 Mbps',  100,  59.99),
    ('Plan Empresarial', 'Internet empresarial 200 Mbps',  200,  99.99);

-- Dispositivos asociados a cada plan
INSERT INTO device (devicename, service_serviceid) VALUES
    ('Router TP-Link N300',   1),
    ('Router TP-Link N450',   2),
    ('Router ASUS AC750',     3),
    ('Router ASUS AC1200',    4),
    ('Router MikroTik hEX',   5);
