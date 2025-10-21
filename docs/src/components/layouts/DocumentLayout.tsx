import React from "react";
import { Outlet, Link } from "react-router-dom";

export default function DocumentLayout() {
    return (
        <>
            <header style={{ padding: 10, borderBottom: "1px solid #ddd" }}>
                <Link to="/">Home</Link> | <Link to="/about">About</Link> | <Link to="/contact">Contact</Link>
            </header>
            <main style={{ padding: 10 }}>
                <Outlet />
            </main>
        </>
    );
}
