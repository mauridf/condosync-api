-- ============================================
-- CondoSync v2.0 - Migration V001
-- Schema inicial completo
-- ============================================

-- Habilitar extensão UUID
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================
-- Schema: admin (domínio global)
-- ============================================
CREATE SCHEMA IF NOT EXISTS admin;

-- Tabela: super_admins
CREATE TABLE admin.super_admins (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    email VARCHAR(200) UNIQUE NOT NULL,
    password_hash VARCHAR(300) NOT NULL,
    role VARCHAR(30) NOT NULL DEFAULT 'super_admin',
    email_verified_at TIMESTAMPTZ,
    last_login_at TIMESTAMPTZ,
    last_password_change_at TIMESTAMPTZ,
    failed_login_attempts INTEGER DEFAULT 0,
    locked_until TIMESTAMPTZ,
    two_factor_enabled BOOLEAN DEFAULT false,
    two_factor_secret VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE UNIQUE INDEX idx_super_admins_email ON admin.super_admins(email);

-- ============================================
-- Schema: public (multi-tenant)
-- ============================================

-- Tabela: condominiums
CREATE TABLE public.condominiums (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    cnpj VARCHAR(18),
    slug VARCHAR(100) UNIQUE NOT NULL,
    address VARCHAR(300),
    city VARCHAR(100),
    state VARCHAR(2),
    zip_code VARCHAR(9),
    phone VARCHAR(20),
    email VARCHAR(200),
    logo_url VARCHAR(500),
    subscription_plan VARCHAR(50) DEFAULT 'trial',
    subscription_status VARCHAR(30) DEFAULT 'trial',
    subscription_expires_at TIMESTAMPTZ,
    trial_ends_at TIMESTAMPTZ,
    max_units INTEGER DEFAULT 0,
    max_residents_per_unit INTEGER DEFAULT 10,
    timezone VARCHAR(50) DEFAULT 'America/Sao_Paulo',
    language VARCHAR(10) DEFAULT 'pt-BR',
    enabled_modules JSONB DEFAULT '["units","residents","notices","tickets"]',
    settings JSONB DEFAULT '{}',
    features JSONB DEFAULT '{}',
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_condominiums_slug ON public.condominiums(slug) WHERE deleted_at IS NULL;
CREATE INDEX idx_condominiums_cnpj ON public.condominiums(cnpj) WHERE cnpj IS NOT NULL;
CREATE INDEX idx_condominiums_status ON public.condominiums(subscription_status);
CREATE INDEX idx_condominiums_trial ON public.condominiums(trial_ends_at) WHERE subscription_plan = 'trial';

-- Tabela: users
CREATE TABLE public.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    name VARCHAR(200) NOT NULL,
    email VARCHAR(200) NOT NULL,
    password_hash VARCHAR(300) NOT NULL,
    phone VARCHAR(20),
    cpf VARCHAR(14),
    avatar_url VARCHAR(500),
    role VARCHAR(30) NOT NULL DEFAULT 'resident',
    email_verified_at TIMESTAMPTZ,
    last_login_at TIMESTAMPTZ,
    last_password_change_at TIMESTAMPTZ,
    failed_login_attempts INTEGER DEFAULT 0,
    locked_until TIMESTAMPTZ,
    two_factor_enabled BOOLEAN DEFAULT false,
    two_factor_secret VARCHAR(100),
    refresh_token TEXT,
    refresh_token_expires_at TIMESTAMPTZ,
    is_active BOOLEAN DEFAULT true,
    notification_preferences JSONB DEFAULT '{"email": true, "push": true, "in_app": true}',
    theme_preferences JSONB DEFAULT '{"mode": "light", "accent_color": "#1976D2"}',
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ
);

CREATE UNIQUE INDEX idx_users_email_condo ON public.users(condominium_id, email) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_role ON public.users(condominium_id, role);
CREATE INDEX idx_users_email ON public.users(email) WHERE deleted_at IS NULL;

-- Tabela: units
CREATE TABLE public.units (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    block VARCHAR(50),
    number VARCHAR(50) NOT NULL,
    floor VARCHAR(20),
    type VARCHAR(50) NOT NULL DEFAULT 'apartment',
    area DECIMAL(10,2),
    bedrooms INTEGER DEFAULT 0,
    bathrooms INTEGER DEFAULT 0,
    parking_spots INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    is_rented BOOLEAN DEFAULT false,
    occupancy_status VARCHAR(30) DEFAULT 'vacant',
    monthly_fee DECIMAL(10,2),
    late_fee_percentage DECIMAL(5,2) DEFAULT 2.00,
    interest_percentage DECIMAL(5,2) DEFAULT 0.033,
    iptu_annual DECIMAL(10,2),
    custom_fields JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    UNIQUE(condominium_id, block, number)
);

CREATE INDEX idx_units_condominium ON public.units(condominium_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_units_occupancy ON public.units(condominium_id, occupancy_status);

-- Tabela: residents
CREATE TABLE public.residents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    unit_id UUID NOT NULL REFERENCES public.units(id),
    user_id UUID REFERENCES public.users(id),
    resident_type VARCHAR(30) NOT NULL,
    name VARCHAR(200) NOT NULL,
    email VARCHAR(200),
    phone VARCHAR(20),
    cpf VARCHAR(14),
    rg VARCHAR(20),
    birth_date DATE,
    profession VARCHAR(100),
    owner_name VARCHAR(200),
    owner_phone VARCHAR(20),
    owner_email VARCHAR(200),
    move_in_date DATE,
    move_out_date DATE,
    is_active BOOLEAN DEFAULT true,
    is_primary BOOLEAN DEFAULT false,
    is_emergency_contact BOOLEAN DEFAULT false,
    vehicles JSONB DEFAULT '[]',
    pets JSONB DEFAULT '[]',
    has_system_access BOOLEAN DEFAULT false,
    access_code VARCHAR(10),
    access_granted_at TIMESTAMPTZ,
    biometric_hash VARCHAR(200),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_residents_unit ON public.residents(unit_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_residents_condo ON public.residents(condominium_id);
CREATE INDEX idx_residents_cpf ON public.residents(cpf) WHERE cpf IS NOT NULL;
CREATE UNIQUE INDEX idx_residents_unit_primary ON public.residents(unit_id) WHERE is_primary = true AND deleted_at IS NULL;

-- Tabela: services
CREATE TABLE public.services (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(100) NOT NULL,
    description TEXT,
    icon VARCHAR(100),
    category VARCHAR(100) NOT NULL,
    service_type VARCHAR(50) NOT NULL,
    requires_approval BOOLEAN DEFAULT false,
    requires_payment BOOLEAN DEFAULT false,
    max_booking_per_day INTEGER,
    max_booking_per_user INTEGER,
    advance_booking_days INTEGER DEFAULT 0,
    cancel_before_hours INTEGER DEFAULT 24,
    allow_simultaneous BOOLEAN DEFAULT false,
    available_days JSONB,
    available_time_start TIME,
    available_time_end TIME,
    slot_duration INTEGER,
    allow_custom_time BOOLEAN DEFAULT false,
    blocked_dates JSONB DEFAULT '[]',
    price DECIMAL(10,2) DEFAULT 0,
    price_unit VARCHAR(20),
    rules JSONB DEFAULT '[]',
    terms_of_use TEXT,
    is_active BOOLEAN DEFAULT true,
    display_order INTEGER DEFAULT 0,
    images JSONB DEFAULT '[]',
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    UNIQUE(condominium_id, slug)
);

CREATE INDEX idx_services_condo_active ON public.services(condominium_id, is_active);

-- Tabela: bookings
CREATE TABLE public.bookings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    service_id UUID NOT NULL REFERENCES public.services(id),
    unit_id UUID NOT NULL REFERENCES public.units(id),
    resident_id UUID NOT NULL REFERENCES public.residents(id),
    booking_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    status VARCHAR(30) DEFAULT 'pending',
    title VARCHAR(300),
    description TEXT,
    guests_count INTEGER DEFAULT 0,
    special_requirements TEXT,
    approved_by UUID REFERENCES public.users(id),
    approved_at TIMESTAMPTZ,
    rejection_reason TEXT,
    rejected_at TIMESTAMPTZ,
    cancelled_by UUID REFERENCES public.users(id),
    cancelled_at TIMESTAMPTZ,
    cancellation_reason TEXT,
    cancelled_by_system BOOLEAN DEFAULT false,
    amount DECIMAL(10,2),
    payment_status VARCHAR(30),
    payment_method VARCHAR(50),
    paid_at TIMESTAMPTZ,
    transaction_id VARCHAR(100),
    checked_in_at TIMESTAMPTZ,
    checked_out_at TIMESTAMPTZ,
    qr_code_url VARCHAR(500),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_bookings_date_service ON public.bookings(booking_date, service_id, status);
CREATE INDEX idx_bookings_resident ON public.bookings(resident_id);
CREATE INDEX idx_bookings_status ON public.bookings(condominium_id, status);
CREATE INDEX idx_bookings_payment ON public.bookings(payment_status) WHERE amount > 0;

-- Tabela: notices
CREATE TABLE public.notices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    author_id UUID NOT NULL REFERENCES public.users(id),
    title VARCHAR(300) NOT NULL,
    content TEXT NOT NULL,
    summary VARCHAR(500),
    category VARCHAR(50) NOT NULL,
    visibility VARCHAR(30) DEFAULT 'all',
    target_units JSONB DEFAULT '[]',
    is_pinned BOOLEAN DEFAULT false,
    is_urgent BOOLEAN DEFAULT false,
    pin_expires_at TIMESTAMPTZ,
    expires_at TIMESTAMPTZ,
    views_count INTEGER DEFAULT 0,
    unique_views_count INTEGER DEFAULT 0,
    attachments JSONB DEFAULT '[]',
    reactions JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    published_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_notices_condo_date ON public.notices(condominium_id, published_at DESC) WHERE published_at IS NOT NULL;
CREATE INDEX idx_notices_category ON public.notices(condominium_id, category);
CREATE INDEX idx_notices_pinned ON public.notices(is_pinned, published_at) WHERE is_pinned = true;

-- Tabela: notice_comments
CREATE TABLE public.notice_comments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    notice_id UUID NOT NULL REFERENCES public.notices(id) ON DELETE CASCADE,
    author_id UUID NOT NULL REFERENCES public.users(id),
    content TEXT NOT NULL,
    is_edited BOOLEAN DEFAULT false,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_notice_comments_notice ON public.notice_comments(notice_id);

-- Tabela: tickets
CREATE TABLE public.tickets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    unit_id UUID NOT NULL REFERENCES public.units(id),
    resident_id UUID NOT NULL REFERENCES public.residents(id),
    assigned_to UUID REFERENCES public.users(id),
    ticket_number VARCHAR(20) NOT NULL,
    title VARCHAR(300) NOT NULL,
    description TEXT NOT NULL,
    category VARCHAR(100) NOT NULL,
    subcategory VARCHAR(100),
    priority VARCHAR(20) DEFAULT 'normal',
    status VARCHAR(30) DEFAULT 'open',
    sla_hours INTEGER DEFAULT 48,
    sla_breached_at TIMESTAMPTZ,
    resolution TEXT,
    resolved_at TIMESTAMPTZ,
    closed_at TIMESTAMPTZ,
    resolved_by UUID REFERENCES public.users(id),
    location_type VARCHAR(50),
    location_description VARCHAR(300),
    rating INTEGER CHECK (rating >= 1 AND rating <= 5),
    feedback TEXT,
    cost DECIMAL(10,2),
    paid_by VARCHAR(30),
    attachments JSONB DEFAULT '[]',
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_tickets_condo_status ON public.tickets(condominium_id, status);
CREATE INDEX idx_tickets_priority ON public.tickets(condominium_id, priority, status);
CREATE INDEX idx_tickets_assigned ON public.tickets(assigned_to, status);
CREATE INDEX idx_tickets_number ON public.tickets(condominium_id, ticket_number);

-- Tabela: ticket_messages
CREATE TABLE public.ticket_messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticket_id UUID NOT NULL REFERENCES public.tickets(id) ON DELETE CASCADE,
    sender_id UUID NOT NULL REFERENCES public.users(id),
    message TEXT NOT NULL,
    is_internal BOOLEAN DEFAULT false,
    is_system_message BOOLEAN DEFAULT false,
    attachments JSONB DEFAULT '[]',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_ticket_messages_ticket ON public.ticket_messages(ticket_id);

-- Tabela: bills
CREATE TABLE public.bills (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    unit_id UUID NOT NULL REFERENCES public.units(id),
    bill_number VARCHAR(50),
    description VARCHAR(300) NOT NULL,
    reference_month VARCHAR(7) NOT NULL,
    base_amount DECIMAL(10,2) NOT NULL,
    discount_amount DECIMAL(10,2) DEFAULT 0,
    discount_type VARCHAR(30),
    fine_amount DECIMAL(10,2) DEFAULT 0,
    interest_amount DECIMAL(10,2) DEFAULT 0,
    total_amount DECIMAL(10,2) NOT NULL,
    balance DECIMAL(10,2),
    issue_date DATE NOT NULL,
    due_date DATE NOT NULL,
    fine_start_date DATE,
    late_fee_percentage DECIMAL(5,2) DEFAULT 2.00,
    late_interest_daily DECIMAL(5,2) DEFAULT 0.033,
    max_interest_months INTEGER DEFAULT 12,
    status VARCHAR(30) DEFAULT 'pending',
    payment_date DATE,
    payment_amount DECIMAL(10,2),
    payment_method VARCHAR(50),
    transaction_id VARCHAR(100),
    paid_by UUID REFERENCES public.residents(id),
    installment_number INTEGER DEFAULT 1,
    total_installments INTEGER DEFAULT 1,
    parent_bill_id UUID REFERENCES public.bills(id),
    boleto_url VARCHAR(500),
    boleto_code VARCHAR(100),
    pix_code VARCHAR(500),
    pix_qr_code_url VARCHAR(500),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_bills_unit_date ON public.bills(unit_id, due_date);
CREATE INDEX idx_bills_status ON public.bills(condominium_id, status);
CREATE INDEX idx_bills_reference ON public.bills(condominium_id, reference_month);
CREATE INDEX idx_bills_overdue ON public.bills(condominium_id, status) WHERE status = 'overdue';

-- Tabela: visitors
CREATE TABLE public.visitors (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    unit_id UUID NOT NULL REFERENCES public.units(id),
    resident_id UUID REFERENCES public.residents(id),
    name VARCHAR(200) NOT NULL,
    document VARCHAR(30),
    document_type VARCHAR(20),
    vehicle_plate VARCHAR(10),
    vehicle_model VARCHAR(100),
    phone VARCHAR(20),
    visit_date DATE NOT NULL,
    entry_time TIMESTAMPTZ,
    exit_time TIMESTAMPTZ,
    expected_entry_time TIME,
    expected_exit_time TIME,
    visitor_type VARCHAR(30) NOT NULL,
    company_name VARCHAR(200),
    service_description VARCHAR(300),
    authorization_code VARCHAR(10),
    qr_code_url VARCHAR(500),
    is_recurring BOOLEAN DEFAULT false,
    recurring_schedule JSONB,
    status VARCHAR(30) DEFAULT 'authorized',
    notes TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_visitors_date ON public.visitors(condominium_id, visit_date);
CREATE UNIQUE INDEX idx_visitors_code ON public.visitors(authorization_code) WHERE authorization_code IS NOT NULL;
CREATE INDEX idx_visitors_unit ON public.visitors(unit_id, visit_date);

-- Tabela: common_areas
CREATE TABLE public.common_areas (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    name VARCHAR(200) NOT NULL,
    description TEXT,
    type VARCHAR(50) NOT NULL,
    capacity INTEGER,
    max_guests_per_resident INTEGER DEFAULT 5,
    rules JSONB DEFAULT '[]',
    requires_booking BOOLEAN DEFAULT false,
    requires_deposit BOOLEAN DEFAULT false,
    deposit_amount DECIMAL(10,2),
    open_time TIME,
    close_time TIME,
    operating_hours JSONB DEFAULT '{}',
    is_active BOOLEAN DEFAULT true,
    maintenance_status VARCHAR(30) DEFAULT 'operational',
    scheduled_maintenance JSONB DEFAULT '[]',
    images JSONB DEFAULT '[]',
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_common_areas_condo ON public.common_areas(condominium_id, is_active);

-- Tabela: condominium_settings
CREATE TABLE public.condominium_settings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id) UNIQUE,
    allow_self_registration BOOLEAN DEFAULT true,
    require_admin_approval BOOLEAN DEFAULT true,
    allow_guest_registration BOOLEAN DEFAULT true,
    max_family_members_per_unit INTEGER DEFAULT 10,
    max_pets_per_unit INTEGER DEFAULT 3,
    invoice_generation_day INTEGER DEFAULT 5,
    due_day INTEGER DEFAULT 10,
    late_fee_percentage DECIMAL(5,2) DEFAULT 2.00,
    late_interest_daily DECIMAL(5,2) DEFAULT 0.033,
    early_payment_discount_percentage DECIMAL(5,2) DEFAULT 0,
    early_payment_days INTEGER DEFAULT 0,
    automatic_boleto_generation BOOLEAN DEFAULT false,
    enable_pix BOOLEAN DEFAULT true,
    enable_credit_card BOOLEAN DEFAULT false,
    payment_gateway JSONB DEFAULT '{}',
    notification_email_template JSONB DEFAULT '{}',
    email_from_name VARCHAR(200),
    email_from_address VARCHAR(200),
    sms_enabled BOOLEAN DEFAULT false,
    sms_provider JSONB DEFAULT '{}',
    primary_color VARCHAR(7) DEFAULT '#1976D2',
    secondary_color VARCHAR(7) DEFAULT '#FF9800',
    logo_url VARCHAR(500),
    favicon_url VARCHAR(500),
    custom_css TEXT,
    visitor_qr_code_enabled BOOLEAN DEFAULT true,
    visitor_notify_owner BOOLEAN DEFAULT true,
    max_visitors_per_day INTEGER DEFAULT 10,
    visitor_auto_approve BOOLEAN DEFAULT false,
    integrations JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Tabela: unit_invitations
CREATE TABLE public.unit_invitations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    unit_id UUID NOT NULL REFERENCES public.units(id),
    invitation_code VARCHAR(50) UNIQUE NOT NULL,
    invitation_url VARCHAR(500),
    expires_at TIMESTAMPTZ,
    max_uses INTEGER DEFAULT 1,
    uses_count INTEGER DEFAULT 0,
    recipient_email VARCHAR(200),
    recipient_name VARCHAR(200),
    recipient_phone VARCHAR(20),
    access_type VARCHAR(30) DEFAULT 'owner',
    status VARCHAR(30) DEFAULT 'active',
    created_by UUID NOT NULL REFERENCES public.users(id),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_invitations_code ON public.unit_invitations(invitation_code);
CREATE INDEX idx_invitations_unit ON public.unit_invitations(unit_id);

-- ============================================
-- Seed: SuperAdmin inicial
-- ============================================
-- Senha: Admin@123!ChangeMe (BCrypt hash)
-- Em produção, alterar via variável de ambiente
INSERT INTO admin.super_admins (name, email, password_hash, role, email_verified_at)
SELECT 
    'Super Admin',
    'admin@condosync.com.br',
    '$2a$12$LJ3m4ys3GZfnYMz8kVsKaOGqPqXEQfWvKdN1o8nOdLzCm4vzKrZqG',
    'super_admin',
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM admin.super_admins WHERE role = 'super_admin');